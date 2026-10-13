namespace Prodora.Entitys;

/// <summary>A delivery address a customer keeps in their address book.</summary>
public class SavedAddress
{
	public int Id { get; set; }
	public string UserId { get; set; } = "";
	public string Title { get; set; } = "";
	public string FirstName { get; set; } = "";
	public string LastName { get; set; } = "";
	public string Phone { get; set; } = "";
	public string Address { get; set; } = "";
	public string City { get; set; } = "";
	public bool IsDefault { get; set; }
	public DateTime CreatedAt { get; set; }
}

public static class AddressRules
{
	public const int MaxPerUser = 10;

	/// <summary>Trims every field and keeps only the digits (and a leading +) of the phone number.</summary>
	public static void Normalize(SavedAddress address)
	{
		address.Title = (address.Title ?? "").Trim();
		address.FirstName = (address.FirstName ?? "").Trim();
		address.LastName = (address.LastName ?? "").Trim();
		address.Address = (address.Address ?? "").Trim();
		address.City = (address.City ?? "").Trim();
		var phone = (address.Phone ?? "").Trim();
		var digits = new string(phone.Where(char.IsAsciiDigit).ToArray());
		address.Phone = phone.StartsWith('+') ? "+" + digits : digits;
	}

	/// <summary>Returns an error message for the address form, or null when the address can be saved.</summary>
	public static string? Validate(SavedAddress address)
	{
		if (address.Title.Length is 0 or > 30) return "Adres başlığı 1 ile 30 karakter arasında olmalı (ör. Ev, İş).";
		if (address.FirstName.Length is 0 or > 50) return "Ad 1 ile 50 karakter arasında olmalı.";
		if (address.LastName.Length is 0 or > 50) return "Soyad 1 ile 50 karakter arasında olmalı.";
		if (address.Address.Length is 0 or > 250) return "Adres 1 ile 250 karakter arasında olmalı.";
		if (address.City.Length is 0 or > 50) return "Şehir 1 ile 50 karakter arasında olmalı.";
		var digits = address.Phone.Count(char.IsAsciiDigit);
		if (digits is < 10 or > 15) return "Geçerli bir telefon numarası gir.";
		return null;
	}
}
