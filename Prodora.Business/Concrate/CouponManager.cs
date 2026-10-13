using Prodora.Business.Abstract;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.Business.Concrate;

public class CouponManager(ICouponDal couponDal) : ICouponServices
{
    public List<Coupon> GetAll() => couponDal.GetAll();
    public void Create(Coupon coupon) => couponDal.Create(coupon);
    public void SetActive(int id, bool isActive) => couponDal.SetActive(id, isActive);

    public CouponCheck Preview(string? code, string userId, decimal subtotal)
    {
        if (string.IsNullOrWhiteSpace(code)) return new(false);
        var coupon = CouponRules.NormalizeCode(code) is { } normalized ? couponDal.GetByCode(normalized) : null;
        var used = coupon != null && couponDal.HasRedeemed(coupon.Id, userId);
        return CouponRules.Evaluate(coupon, subtotal, DateTime.Now, used);
    }
}
