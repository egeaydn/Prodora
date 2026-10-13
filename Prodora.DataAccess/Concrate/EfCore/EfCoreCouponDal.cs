using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore;

public class EfCoreCouponDal(IDbContextFactory<DataContext> factory) : ICouponDal
{
    public List<Coupon> GetAll()
    {
        using var context = factory.CreateDbContext();
        return context.Coupons.AsNoTracking().OrderByDescending(c => c.IsActive).ThenBy(c => c.Code).ToList();
    }

    public Coupon? GetByCode(string code)
    {
        var normalized = CouponRules.NormalizeCode(code);
        if (normalized == null) return null;
        using var context = factory.CreateDbContext();
        return context.Coupons.AsNoTracking().SingleOrDefault(c => c.Code == normalized);
    }

    public bool HasRedeemed(int couponId, string userId)
    {
        using var context = factory.CreateDbContext();
        return context.CouponRedemptions.Any(r => r.CouponId == couponId && r.UserId == userId);
    }

    public void Create(Coupon coupon)
    {
        var error = CouponRules.ValidateDefinition(coupon);
        if (error != null) throw new StoreValidationException(error);
        coupon.Code = CouponRules.NormalizeCode(coupon.Code)!;
        coupon.RedemptionCount = 0;
        using var context = factory.CreateDbContext();
        if (context.Coupons.Any(c => c.Code == coupon.Code)) throw new StoreValidationException("Bu kupon kodu zaten var.");
        context.Coupons.Add(coupon);
        context.SaveChanges();
    }

    public void SetActive(int id, bool isActive)
    {
        using var context = factory.CreateDbContext();
        var coupon = context.Coupons.Find(id) ?? throw new StoreValidationException("Kupon bulunamadı.");
        coupon.IsActive = isActive;
        context.SaveChanges();
    }
}
