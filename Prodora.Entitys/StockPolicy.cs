namespace Prodora.Entitys;

/// <summary>
/// Stock rules. A product either tracks a quantity (<see cref="Product.StockQuantity"/> has a value) or it does not
/// (null, the original on/off behaviour). For a tracked product <see cref="Product.Stock"/> always mirrors "quantity above zero",
/// so every place that only asks "is it in stock?" keeps working.
/// </summary>
public static class StockPolicy
{
	public const int LowStockThreshold = 5;
	public const int MaxQuantity = 100000;
	public const int MaxPerOrderLine = 99;

	public static string? ValidateQuantity(int? quantity) =>
		quantity is < 0 or > MaxQuantity ? $"Stok adedi 0 ile {MaxQuantity} arasında olmalı." : null;

	/// <summary>Applies the admin's input: a quantity turns tracking on, a blank quantity turns it off and the on/off flag decides.</summary>
	public static void Apply(Product product, int? quantity, bool inStockWhenUntracked)
	{
		product.StockQuantity = quantity;
		product.Stock = quantity is { } tracked ? tracked > 0 : inStockWhenUntracked;
	}

	/// <summary>Tracked, on sale, and at or below the warning level (this includes sold out).</summary>
	public static bool IsLow(Product product) => !product.IsArchived && product.StockQuantity is { } quantity && quantity <= LowStockThreshold;

	/// <summary>Most units a customer can put in one basket line right now.</summary>
	public static int MaxPurchasable(Product product) =>
		product.StockQuantity is { } quantity ? Math.Clamp(quantity, 0, MaxPerOrderLine) : MaxPerOrderLine;
}
