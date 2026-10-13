using Prodora.Business.Abstract;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.Business.Concrate;

public class WishlistManager(IWishlistDal wishlistDal) : IWishlistServices
{
    public List<Product> GetProducts(string userId) => wishlistDal.GetProducts(userId);
    public HashSet<int> GetProductIds(string userId) => wishlistDal.GetProductIds(userId);
    public bool Toggle(string userId, int productId) => wishlistDal.Toggle(userId, productId);
}
