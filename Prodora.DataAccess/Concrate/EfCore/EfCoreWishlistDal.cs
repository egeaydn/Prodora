using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore;

public class EfCoreWishlistDal(IDbContextFactory<DataContext> factory) : IWishlistDal
{
    public List<Product> GetProducts(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return new();
        using var context = factory.CreateDbContext();
        // Newest first; the products are loaded separately because Include cannot follow a Select.
        var orderedIds = context.WishlistItems.AsNoTracking()
            .Where(w => w.UserId == userId && !w.Product.IsArchived)
            .OrderByDescending(w => w.AddedAt).ThenByDescending(w => w.Id)
            .Select(w => w.ProductId).ToList();
        var products = context.Products.AsNoTracking().Include(p => p.Images).Where(p => orderedIds.Contains(p.Id)).ToDictionary(p => p.Id);
        return orderedIds.Where(products.ContainsKey).Select(id => products[id]).ToList();
    }

    public HashSet<int> GetProductIds(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return new();
        using var context = factory.CreateDbContext();
        return context.WishlistItems.AsNoTracking().Where(w => w.UserId == userId).Select(w => w.ProductId).ToHashSet();
    }

    public bool Toggle(string userId, int productId)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new StoreValidationException("Favorilere eklemek için giriş yap.");
        using var context = factory.CreateDbContext();
        var existing = context.WishlistItems.FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);
        if (existing != null)
        {
            context.WishlistItems.Remove(existing);
            context.SaveChanges();
            return false;
        }
        var product = context.Products.AsNoTracking().FirstOrDefault(p => p.Id == productId);
        if (product == null || product.IsArchived) throw new StoreValidationException("Bu ürün artık satışta değil.");
        if (context.WishlistItems.Count(w => w.UserId == userId) >= WishlistItem.MaxPerUser)
            throw new StoreValidationException($"Favori listene en fazla {WishlistItem.MaxPerUser} ürün ekleyebilirsin.");
        context.WishlistItems.Add(new WishlistItem { UserId = userId, ProductId = productId, AddedAt = DateTime.Now });
        try { context.SaveChanges(); }
        catch (DbUpdateException)
        {
            // A parallel request saved the same product first; the unique index kept it to one row, which is the result asked for.
        }
        return true;
    }
}
