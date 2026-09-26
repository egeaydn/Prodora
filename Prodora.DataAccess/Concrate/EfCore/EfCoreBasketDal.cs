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
    public void AddItem(string userId, int productId, int quantity)
    {
        if (quantity < 1 || quantity > 99) throw new StoreValidationException("Ürün adedi 1 ile 99 arasında olmalı.");
        using var context = _contextFactory.CreateDbContext();
        using var transaction = CommerceTransaction.Begin(context, userId);
        CommerceTransaction.EnsureEditable(context, userId);
        var product = context.Products.SingleOrDefault(p => p.Id == productId);
        if (product == null || product.IsArchived || !product.Stock || product.Price <= 0)
            throw new StoreValidationException("Bu ürün şu anda satışta değil.");
        var basket = context.Baskets.Include(b => b.BasketItems).OrderBy(b => b.Id).FirstOrDefault(b => b.UserId == userId);
        if (basket == null) { basket = new Basket { UserId = userId, BasketItems = new() }; context.Baskets.Add(basket); }
        var items = basket.BasketItems.Where(i => i.ProductId == productId).ToList();
        var total = items.Sum(i => (long)i.Quantity) + quantity;
        if (total > 99 || total < 1) throw new StoreValidationException("Bir üründen sepete en fazla 99 adet ekleyebilirsin.");
        if (items.Count == 0) basket.BasketItems.Add(new BasketItem { ProductId = productId, Quantity = quantity });
        else
        {
            items[0].Quantity = (int)total;
            context.RemoveRange(items.Skip(1));
        }
        context.SaveChanges();
        transaction.Commit();
    }
    public void DeleteFromCart(int basketId, int productId) => RemoveItems(basketId, productId);
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
