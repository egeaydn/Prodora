using Microsoft.EntityFrameworkCore;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore;

/// <summary>
/// Holds stock for an order from the moment it is placed (so a customer who is paying keeps their items) and gives it back when
/// the payment fails or the order is cancelled. What was actually held is recorded per order item, so a release is exact and
/// can never be applied twice. Callers run it inside the checkout/settlement transaction.
/// Stock lives on the product (a plain quantity) or, for products bought by choosing a size/colour, on each variant.
/// </summary>
internal static class StockLedger
{
    public static List<int> ProductIdsInBasket(DataContext context, string userId) =>
        context.Baskets.Where(b => b.UserId == userId).SelectMany(b => b.BasketItems).Select(i => i.ProductId).Distinct().ToList();

    /// <summary>
    /// One application lock per product, always in ascending id order, so buyers of the same product (and of its variants)
    /// queue up instead of deadlocking when they both read a product and then try to change it.
    /// </summary>
    public static void LockProducts(DataContext context, IEnumerable<int> productIds)
    {
        foreach (var id in productIds.Distinct().OrderBy(i => i))
        {
            var resource = "Prodora:stock:" + id;
            context.Database.ExecuteSqlInterpolated($@"DECLARE @result int;
                EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000;
                IF @result < 0 THROW 51000, 'Stock lock unavailable', 1;");
        }
    }

    /// <summary>
    /// A product that has variants is in stock when an active variant has units left, and then has no quantity of its own.
    /// Call it after any change to a variant's stock or switch.
    /// </summary>
    public static void RefreshProductFlag(DataContext context, int productId) =>
        context.Database.ExecuteSqlInterpolated($@"UPDATE ProdoraProducts
            SET Stock = CASE WHEN EXISTS (SELECT 1 FROM ProductVariants v WHERE v.ProductId = {productId} AND v.IsActive = 1 AND v.StockQuantity > 0) THEN 1 ELSE 0 END,
                StockQuantity = NULL
            WHERE Id = {productId} AND EXISTS (SELECT 1 FROM ProductVariants v WHERE v.ProductId = {productId})");

    /// <summary>Takes the ordered quantities off tracked products and variants. Throws when something does not have enough left.</summary>
    public static void Reserve(DataContext context, Order draft, IReadOnlyDictionary<int, Product> products)
    {
        var touched = new HashSet<int>();
        foreach (var line in draft.OrderItems.GroupBy(i => (i.ProductId, i.VariantId)).OrderBy(g => g.Key.ProductId).ThenBy(g => g.Key.VariantId))
        {
            var product = products[line.Key.ProductId];
            var quantity = line.Sum(i => i.Quantity);
            if (line.Key.VariantId is { } variantId)
            {
                // One conditional statement: it only succeeds while enough is left, whatever other buyers do meanwhile.
                var taken = context.Database.ExecuteSqlInterpolated($@"UPDATE ProductVariants SET StockQuantity = StockQuantity - {quantity}
                    WHERE Id = {variantId} AND ProductId = {product.Id} AND IsActive = 1 AND StockQuantity >= {quantity}");
                if (taken == 0)
                {
                    var left = context.ProductVariants.AsNoTracking().Where(v => v.Id == variantId && v.IsActive).Select(v => (int?)v.StockQuantity).FirstOrDefault() ?? 0;
                    var label = $"\"{product.Name} · {line.First().VariantName}\"";
                    throw new StoreValidationException(left > 0 ? $"{label} için yalnızca {left} adet kaldı. Sepetini güncelle." : $"{label} tükendi. Sepetinden çıkar.");
                }
                touched.Add(product.Id);
            }
            else if (product.StockQuantity != null)
            {
                var taken = context.Database.ExecuteSqlInterpolated($@"UPDATE ProdoraProducts
                    SET StockQuantity = StockQuantity - {quantity}, Stock = CASE WHEN StockQuantity - {quantity} > 0 THEN 1 ELSE 0 END
                    WHERE Id = {line.Key.ProductId} AND StockQuantity IS NOT NULL AND StockQuantity >= {quantity}");
                if (taken == 0)
                {
                    var left = context.Products.AsNoTracking().Where(p => p.Id == line.Key.ProductId).Select(p => p.StockQuantity).FirstOrDefault() ?? 0;
                    throw new StoreValidationException(left > 0
                        ? $"\"{product.Name}\" için yalnızca {left} adet kaldı. Sepetini güncelle."
                        : $"\"{product.Name}\" tükendi. Sepetinden çıkar.");
                }
            }
            else continue;
            foreach (var item in line) item.StockReserved = item.Quantity;
        }
        foreach (var productId in touched) RefreshProductFlag(context, productId);
    }

    /// <summary>Returns what the order held. The order's items must be loaded and tracked.</summary>
    public static void Release(DataContext context, Order order)
    {
        var held = order.OrderItems.Where(i => i.StockReserved > 0)
            .GroupBy(i => (i.ProductId, i.VariantId)).OrderBy(g => g.Key.ProductId).ThenBy(g => g.Key.VariantId).ToList();
        if (held.Count == 0) return;
        LockProducts(context, held.Select(g => g.Key.ProductId));
        var touched = new HashSet<int>();
        foreach (var line in held)
        {
            var quantity = line.Sum(i => i.StockReserved);
            if (line.Key.VariantId is { } variantId)
            {
                context.Database.ExecuteSqlInterpolated($"UPDATE ProductVariants SET StockQuantity = StockQuantity + {quantity} WHERE Id = {variantId}");
                touched.Add(line.Key.ProductId);
            }
            else
            {
                // A product that is no longer tracked has nothing to give back to.
                context.Database.ExecuteSqlInterpolated($@"UPDATE ProdoraProducts SET StockQuantity = StockQuantity + {quantity}, Stock = 1
                    WHERE Id = {line.Key.ProductId} AND StockQuantity IS NOT NULL");
            }
            foreach (var item in line) item.StockReserved = 0;
        }
        foreach (var productId in touched) RefreshProductFlag(context, productId);
    }
}
