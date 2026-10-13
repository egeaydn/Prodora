using Microsoft.EntityFrameworkCore;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore;

/// <summary>
/// Holds stock for an order from the moment it is placed (so a customer who is paying keeps their items) and gives it back when
/// the payment fails or the order is cancelled. What was actually held is recorded per order item, so a release is exact and
/// can never be applied twice. Callers run it inside the checkout/settlement transaction.
/// </summary>
internal static class StockLedger
{
    public static List<int> ProductIdsInBasket(DataContext context, string userId) =>
        context.Baskets.Where(b => b.UserId == userId).SelectMany(b => b.BasketItems).Select(i => i.ProductId).Distinct().ToList();

    /// <summary>
    /// One application lock per product, always in ascending id order, so buyers of the same product queue up instead of
    /// deadlocking when they both read a product and then try to change it.
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

    /// <summary>Takes the ordered quantities off tracked products. Throws when a product does not have enough left.</summary>
    public static void Reserve(DataContext context, Order draft, IReadOnlyDictionary<int, Product> products)
    {
        foreach (var line in draft.OrderItems.GroupBy(i => i.ProductId).OrderBy(g => g.Key))
        {
            var product = products[line.Key];
            if (product.StockQuantity == null) continue;
            var quantity = line.Sum(i => i.Quantity);
            // One conditional statement: it only succeeds while enough is left, whatever other buyers do meanwhile.
            var taken = context.Database.ExecuteSqlInterpolated($@"UPDATE ProdoraProducts
                SET StockQuantity = StockQuantity - {quantity}, Stock = CASE WHEN StockQuantity - {quantity} > 0 THEN 1 ELSE 0 END
                WHERE Id = {line.Key} AND StockQuantity IS NOT NULL AND StockQuantity >= {quantity}");
            if (taken == 0)
            {
                var left = context.Products.AsNoTracking().Where(p => p.Id == line.Key).Select(p => p.StockQuantity).FirstOrDefault() ?? 0;
                throw new StoreValidationException(left > 0
                    ? $"\"{product.Name}\" için yalnızca {left} adet kaldı. Sepetini güncelle."
                    : $"\"{product.Name}\" tükendi. Sepetinden çıkar.");
            }
            foreach (var item in line) item.StockReserved = item.Quantity;
        }
    }

    /// <summary>Returns what the order held. The order's items must be loaded and tracked.</summary>
    public static void Release(DataContext context, Order order)
    {
        var held = order.OrderItems.Where(i => i.StockReserved > 0).GroupBy(i => i.ProductId).OrderBy(g => g.Key).ToList();
        if (held.Count == 0) return;
        LockProducts(context, held.Select(g => g.Key));
        foreach (var line in held)
        {
            var quantity = line.Sum(i => i.StockReserved);
            // A product that is no longer tracked has nothing to give back to.
            context.Database.ExecuteSqlInterpolated($@"UPDATE ProdoraProducts SET StockQuantity = StockQuantity + {quantity}, Stock = 1
                WHERE Id = {line.Key} AND StockQuantity IS NOT NULL");
            foreach (var item in line) item.StockReserved = 0;
        }
    }
}
