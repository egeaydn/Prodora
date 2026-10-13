using Prodora.Entitys;

namespace Prodora.WebUI.Reports;

public sealed record LowStockLine(int ProductId, string Name, int Quantity, bool IsVariant);

public static class LowStock
{
    /// <summary>Products (and, for products sold by choice, their active variants) at or below the warning level, emptiest first.</summary>
    public static List<LowStockLine> Find(IEnumerable<Product> products)
    {
        var lines = new List<LowStockLine>();
        foreach (var product in products.Where(p => !p.IsArchived))
        {
            if (VariantRules.HasChoices(product))
                lines.AddRange(product.Variants.Where(v => v.IsActive && v.StockQuantity <= StockPolicy.LowStockThreshold)
                    .Select(v => new LowStockLine(product.Id, $"{product.Name} · {v.Name}", v.StockQuantity, true)));
            else if (StockPolicy.IsLow(product))
                lines.Add(new LowStockLine(product.Id, product.Name, product.StockQuantity!.Value, false));
        }
        return lines.OrderBy(l => l.Quantity).ThenBy(l => l.Name).ToList();
    }
}
