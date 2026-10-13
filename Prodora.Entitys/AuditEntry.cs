namespace Prodora.Entitys;

/// <summary>Who did what in the admin area and when. Rows are only ever added.</summary>
public class AuditEntry
{
	public const int ActionLength = 64;
	public const int TargetLength = 128;
	public const int DetailLength = 500;
	public const int ActorNameLength = 256;
	public const int IpLength = 64;

	public int Id { get; set; }
	public DateTime At { get; set; }
	public string ActorId { get; set; } = "";
	public string ActorName { get; set; } = "";
	/// <summary>Dotted name such as "order.status" or "coupon.create".</summary>
	public string Action { get; set; } = "";
	public string Target { get; set; } = "";
	public string Detail { get; set; } = "";
	public string Ip { get; set; } = "";

	/// <summary>Makes text safe to store and show: no line breaks (which could fake extra log lines) and within the column length.</summary>
	public static string Clean(string? text, int maxLength)
	{
		if (string.IsNullOrEmpty(text)) return "";
		var flat = new string(text.Select(c => char.IsControl(c) ? ' ' : c).ToArray()).Trim();
		return flat.Length <= maxLength ? flat : flat[..(maxLength - 1)] + "…";
	}
}
