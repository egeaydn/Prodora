using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore;

public class EfCoreVariantDal(IDbContextFactory<DataContext> factory) : IVariantDal
{
    public List<ProductVariant> GetByProduct(int productId)
    {
        using var context = factory.CreateDbContext();
        return context.ProductVariants.AsNoTracking().Where(v => v.ProductId == productId).OrderBy(v => v.Id).ToList();
    }

    public ProductVariant? Get(int variantId)
    {
        using var context = factory.CreateDbContext();
        return context.ProductVariants.AsNoTracking().FirstOrDefault(v => v.Id == variantId);
    }

    public ProductVariant Add(int productId, string name, int quantity)
    {
        name = VariantRules.NormalizeName(name);
        var error = VariantRules.Validate(name, quantity);
        if (error != null) throw new StoreValidationException(error);
        return DeadlockRetry.Run(() =>
        {
            using var context = factory.CreateDbContext();
            using var transaction = context.Database.BeginTransaction();
            // Same lock as checkout takes, so a variant cannot appear halfway through someone's order.
            StockLedger.LockProducts(context, new[] { productId });
            if (!context.Products.Any(p => p.Id == productId)) throw new StoreValidationException("Ürün bulunamadı.");
            var existing = context.ProductVariants.Where(v => v.ProductId == productId).Select(v => v.Name).ToList();
            if (existing.Count >= ProductVariant.MaxPerProduct)
                throw new StoreValidationException($"Bir ürüne en fazla {ProductVariant.MaxPerProduct} seçenek ekleyebilirsin.");
            if (existing.Any(n => string.Equals(n, name, StringComparison.CurrentCultureIgnoreCase)))
                throw new StoreValidationException("Bu seçenek zaten var.");
            var variant = new ProductVariant { ProductId = productId, Name = name, StockQuantity = quantity, IsActive = true };
            context.ProductVariants.Add(variant);
            context.SaveChanges();
            StockLedger.RefreshProductFlag(context, productId);
            transaction.Commit();
            return variant;
        });
    }

    public void SetStock(int variantId, int quantity)
    {
        var error = StockPolicy.ValidateQuantity(quantity);
        if (error != null) throw new StoreValidationException(error);
        Change(variantId, variant => variant.StockQuantity = quantity);
    }

    public void SetActive(int variantId, bool isActive) => Change(variantId, variant => variant.IsActive = isActive);

    private void Change(int variantId, Action<ProductVariant> change) => DeadlockRetry.Run(() =>
    {
        using var context = factory.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();
        var productId = context.ProductVariants.AsNoTracking().Where(v => v.Id == variantId).Select(v => (int?)v.ProductId).FirstOrDefault()
            ?? throw new StoreValidationException("Seçenek bulunamadı.");
        StockLedger.LockProducts(context, new[] { productId });
        var variant = context.ProductVariants.First(v => v.Id == variantId);
        change(variant);
        context.SaveChanges();
        StockLedger.RefreshProductFlag(context, productId);
        transaction.Commit();
    });
}
