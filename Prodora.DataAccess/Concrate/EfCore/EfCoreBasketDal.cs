using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;
namespace Prodora.DataAccess.Concrate.EfCore;

public class EfCoreBasketDal(IDbContextFactory<DataContext> factory)
    : EfCoreGenericRepository<Basket, DataContext>(factory), IBasketDal
{
    public Basket CartByUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null!;
        using var context = _contextFactory.CreateDbContext();
        return context.Baskets.Include(b => b.BasketItems).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(b => b.BasketItems).ThenInclude(i => i.Variant)
            .OrderBy(b => b.Id).FirstOrDefault(b => b.UserId == userId)!;
    }
    public void EnsureBasket(string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        using var transaction = CommerceTransaction.Begin(context, userId);
        if (!context.Baskets.Any(b => b.UserId == userId))
        {
            context.Baskets.Add(new Basket { UserId = userId, BasketItems = new() });
            context.SaveChanges();
        }
        transaction.Commit();
    }
    public void AddItem(string userId, int productId, int quantity, int? variantId = null)
    {
        if (quantity < 1 || quantity > 99) throw new StoreValidationException("Ürün adedi 1 ile 99 arasında olmalı.");
        using var context = _contextFactory.CreateDbContext();
        using var transaction = CommerceTransaction.Begin(context, userId);
        CommerceTransaction.EnsureEditable(context, userId);
        var product = context.Products.Include(p => p.Variants).SingleOrDefault(p => p.Id == productId);
        if (product == null || product.IsArchived || !product.Stock || product.Price <= 0)
            throw new StoreValidationException("Bu ürün şu anda satışta değil.");
        ProductVariant? variant = null;
        if (VariantRules.HasChoices(product))
        {
            variant = product.Variants.FirstOrDefault(v => v.Id == variantId && v.IsActive)
                ?? throw new StoreValidationException("Önce bir seçenek (beden, renk…) seç.");
            if (variant.StockQuantity <= 0) throw new StoreValidationException($"\"{variant.Name}\" seçeneği tükendi.");
        }
        else if (variantId != null) throw new StoreValidationException("Bu ürün için seçenek yok.");
        var basket = context.Baskets.Include(b => b.BasketItems).OrderBy(b => b.Id).FirstOrDefault(b => b.UserId == userId);
        if (basket == null) { basket = new Basket { UserId = userId, BasketItems = new() }; context.Baskets.Add(basket); }
        var items = basket.BasketItems.Where(i => i.ProductId == productId && i.VariantId == variant?.Id).ToList();
        var total = items.Sum(i => (long)i.Quantity) + quantity;
        // The 99-unit limit is per product, across all of its variants.
        var productTotal = basket.BasketItems.Where(i => i.ProductId == productId).Sum(i => (long)i.Quantity) + quantity;
        if (total > 99 || total < 1 || productTotal > 99) throw new StoreValidationException("Bir üründen sepete en fazla 99 adet ekleyebilirsin.");
        if (variant != null && total > variant.StockQuantity)
            throw new StoreValidationException($"\"{variant.Name}\" seçeneğinden yalnızca {variant.StockQuantity} adet kaldı.");
        if (product.StockQuantity is { } left && total > left)
            throw new StoreValidationException($"Bu üründen yalnızca {left} adet kaldı.");
        if (items.Count == 0) basket.BasketItems.Add(new BasketItem { ProductId = productId, VariantId = variant?.Id, Quantity = quantity });
        else
        {
            items[0].Quantity = (int)total;
            context.RemoveRange(items.Skip(1));
        }
        context.SaveChanges();
        transaction.Commit();
    }
    public void DeleteFromCart(int basketId, int productId) => RemoveItems(basketId, productId);
    public void DeleteItem(int basketId, int basketItemId)
    {
        using var context = _contextFactory.CreateDbContext();
        var userId = context.Baskets.Where(b => b.Id == basketId).Select(b => b.UserId).SingleOrDefault();
        if (userId == null) return;
        using var transaction = CommerceTransaction.Begin(context, userId);
        CommerceTransaction.EnsureEditable(context, userId);
        context.RemoveRange(context.Set<BasketItem>().Where(i => i.BasketId == basketId && i.Id == basketItemId));
        context.SaveChanges();
        transaction.Commit();
    }
    public void ClearFrommCart(string cartId)
    {
        if (int.TryParse(cartId, out var id)) RemoveItems(id, null);
    }
    private void RemoveItems(int basketId, int? productId)
    {
        using var context = _contextFactory.CreateDbContext();
        var userId = context.Baskets.Where(b => b.Id == basketId).Select(b => b.UserId).SingleOrDefault();
        if (userId == null) return;
        using var transaction = CommerceTransaction.Begin(context, userId);
        CommerceTransaction.EnsureEditable(context, userId);
        context.RemoveRange(context.Set<BasketItem>().Where(i => i.BasketId == basketId && (productId == null || i.ProductId == productId)));
        context.SaveChanges();
        transaction.Commit();
    }
}
