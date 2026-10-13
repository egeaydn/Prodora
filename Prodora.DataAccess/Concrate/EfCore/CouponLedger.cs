using Microsoft.EntityFrameworkCore;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore;

/// <summary>Claims and releases coupon uses. Callers run it inside the transaction that saves or settles the order.</summary>
internal static class CouponLedger
{
    public static void Redeem(DataContext context, Order draft, decimal subtotal)
    {
        if (string.IsNullOrWhiteSpace(draft.CouponCode)) { draft.CouponCode = null; draft.DiscountAmount = 0; return; }
        var code = CouponRules.NormalizeCode(draft.CouponCode) ?? throw new StoreValidationException("Bu kupon kodu geçerli değil.");
        // UPDLOCK makes concurrent buyers of one coupon queue up. Reading with a shared lock and then
        // updating would deadlock under the checkout's RepeatableRead isolation.
        var coupon = context.Coupons.FromSqlInterpolated($"SELECT * FROM Coupons WITH (UPDLOCK, ROWLOCK) WHERE Code = {code}")
            .AsNoTracking().AsEnumerable().SingleOrDefault();
        var used = coupon != null && context.CouponRedemptions.Any(r => r.CouponId == coupon.Id && r.UserId == draft.UserId);
        var check = CouponRules.Evaluate(coupon, subtotal, DateTime.Now, used);
        if (!check.IsValid) throw new StoreValidationException(check.Error!);
        // One atomic statement claims the use, so two buyers racing for the last one cannot both win.
        var claimed = context.Database.ExecuteSqlInterpolated(
            $"UPDATE Coupons SET RedemptionCount = RedemptionCount + 1 WHERE Id = {coupon!.Id} AND IsActive = 1 AND (MaxRedemptions IS NULL OR RedemptionCount < MaxRedemptions)");
        if (claimed == 0) throw new StoreValidationException("Bu kuponun kullanım hakkı bitti.");
        draft.CouponCode = coupon.Code;
        draft.DiscountAmount = check.Discount;
        context.CouponRedemptions.Add(new CouponRedemption { CouponId = coupon.Id, UserId = draft.UserId, Order = draft, RedeemedAt = DateTime.Now });
    }

    /// <summary>Gives the use back when an order fails or is cancelled, so the customer can try again.</summary>
    public static void Release(DataContext context, int orderId)
    {
        var redemption = context.CouponRedemptions.SingleOrDefault(r => r.OrderId == orderId);
        if (redemption == null) return;
        context.CouponRedemptions.Remove(redemption);
        context.Database.ExecuteSqlInterpolated($"UPDATE Coupons SET RedemptionCount = RedemptionCount - 1 WHERE Id = {redemption.CouponId} AND RedemptionCount > 0");
    }
}
