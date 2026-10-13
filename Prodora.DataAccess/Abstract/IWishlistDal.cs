using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract;

public interface IWishlistDal
{
    /// <summary>The customer's saved products, newest first. Archived products are left out.</summary>
    List<Product> GetProducts(string userId);
    HashSet<int> GetProductIds(string userId);
    /// <summary>Adds the product if it is not saved yet, removes it if it is. Returns true when it is now saved.</summary>
    bool Toggle(string userId, int productId);
}
