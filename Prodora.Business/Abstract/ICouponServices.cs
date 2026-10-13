using Prodora.Entitys;

namespace Prodora.Business.Abstract;

public interface ICouponServices
{
    List<Coupon> GetAll();
    void Create(Coupon coupon);
    void SetActive(int id, bool isActive);
    /// <summary>Shows what a code would do to a basket, without using it up.</summary>
    CouponCheck Preview(string? code, string userId, decimal subtotal);
}
