namespace Prodora.Entitys;

/// <summary>
/// A way to buy a product, such as a size or a colour ("M", "Siyah / 42"), with its own stock.
/// The price is the product's. Variants are switched off rather than deleted so past orders keep making sense.
/// </summary>
public class ProductVariant
{
	public const int NameLength = 60;
	public const int MaxPerProduct = 30;

	public int Id { get; set; }
	public int ProductId { get; set; }
	public Product Product { get; set; } = null!;
	public string Name { get; set; } = "";
	public int StockQuantity { get; set; }
	public bool IsActive { get; set; } = true;
}

public static class VariantRules
{
	/// <summary>Trims and collapses inner whitespace so "  M   Siyah " and "M Siyah" are the same variant.</summary>
	public static string NormalizeName(string? name) =>
		string.Join(' ', (name ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

	/// <summary>Returns an error message for the admin form, or null when the variant can be saved.</summary>
	public static string? Validate(string normalizedName, int quantity)
	{
		if (normalizedName.Length is 0 or > ProductVariant.NameLength) return $"Varyant adı 1 ile {ProductVariant.NameLength} karakter arasında olmalı (ör. M, Siyah / 42).";
		if (normalizedName.Any(char.IsControl)) return "Varyant adı özel karakter içeremez.";
		return StockPolicy.ValidateQuantity(quantity);
	}

	/// <summary>A product with at least one active variant is bought by choosing a variant.</summary>
	public static bool HasChoices(Product product) => product.Variants?.Any(v => v.IsActive) == true;

	/// <summary>A product with variants is in stock when any active variant has units left.</summary>
	public static bool AnyInStock(IEnumerable<ProductVariant> variants) => variants.Any(v => v.IsActive && v.StockQuantity > 0);
}
