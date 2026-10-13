using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract;

public interface IVariantDal
{
    /// <summary>All variants of a product, active or not, in the order they were added.</summary>
    List<ProductVariant> GetByProduct(int productId);
    ProductVariant? Get(int variantId);
    /// <summary>Adds a variant. The product's own on/off flag and quantity then follow its variants.</summary>
    ProductVariant Add(int productId, string name, int quantity);
    void SetStock(int variantId, int quantity);
    void SetActive(int variantId, bool isActive);
}
