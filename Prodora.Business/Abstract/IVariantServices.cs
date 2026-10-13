using Prodora.Entitys;

namespace Prodora.Business.Abstract;

public interface IVariantServices
{
    List<ProductVariant> GetByProduct(int productId);
    ProductVariant? Get(int variantId);
    ProductVariant Add(int productId, string name, int quantity);
    void SetStock(int variantId, int quantity);
    void SetActive(int variantId, bool isActive);
}
