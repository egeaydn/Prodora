using Prodora.Business.Abstract;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.Business.Concrate;

public class VariantManager(IVariantDal variantDal) : IVariantServices
{
    public List<ProductVariant> GetByProduct(int productId) => variantDal.GetByProduct(productId);
    public ProductVariant? Get(int variantId) => variantDal.Get(variantId);
    public ProductVariant Add(int productId, string name, int quantity) => variantDal.Add(productId, name, quantity);
    public void SetStock(int variantId, int quantity) => variantDal.SetStock(variantId, quantity);
    public void SetActive(int variantId, bool isActive) => variantDal.SetActive(variantId, isActive);
}
