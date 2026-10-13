using Prodora.Entitys;

namespace Prodora.Business.Abstract;

public interface IWishlistServices
{
    List<Product> GetProducts(string userId);
    HashSet<int> GetProductIds(string userId);
    bool Toggle(string userId, int productId);
}
