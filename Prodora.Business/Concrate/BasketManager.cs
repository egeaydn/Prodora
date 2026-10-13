using Prodora.Business.Abstract;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;
namespace Prodora.Business.Concrate;
public class BasketManager(IBasketDal basketDal) : IBasketServices
{
    public void AddToBasket(string userId, int productId, int quantity, int? variantId = null) => basketDal.AddItem(userId, productId, quantity, variantId);
    public void RemoveItem(string userId, int basketItemId)
    {
        var basket = GetBasketByUserId(userId);
        if (basket != null) basketDal.DeleteItem(basket.Id, basketItemId);
    }
    public void ClearBasket(string basketId) => basketDal.ClearFrommCart(basketId);
    public void DeleteFromBasket(string userId, int productId)
    {
        var basket = GetBasketByUserId(userId);
        if (basket != null) basketDal.DeleteFromCart(basket.Id, productId);
    }
    public Basket GetBasketByUserId(string userId) => basketDal.CartByUserId(userId);
    public void InitialBasket(string userId) => basketDal.EnsureBasket(userId);
}
