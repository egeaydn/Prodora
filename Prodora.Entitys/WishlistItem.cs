namespace Prodora.Entitys;

/// <summary>A product a customer saved for later. One row per customer and product.</summary>
public class WishlistItem
{
	public const int MaxPerUser = 200;

	public int Id { get; set; }
	public string UserId { get; set; } = "";
	public int ProductId { get; set; }
	public Product Product { get; set; } = null!;
	public DateTime AddedAt { get; set; }
}
