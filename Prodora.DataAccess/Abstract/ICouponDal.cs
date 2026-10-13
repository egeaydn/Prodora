using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract;

public interface ICouponDal
{
    List<Coupon> GetAll();
    Coupon? GetByCode(string code);
    bool HasRedeemed(int couponId, string userId);
    void Create(Coupon coupon);
    void SetActive(int id, bool isActive);
}
