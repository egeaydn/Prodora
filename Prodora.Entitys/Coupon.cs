namespace Prodora.Entitys;

public enum CouponKind
{
	Percent = 0,
	FixedAmount = 1
}

public class Coupon
{
	public int Id { get; set; }
	public string Code { get; set; } = "";
	public CouponKind Kind { get; set; }
	/// <summary>A percentage (1-90) for <see cref="CouponKind.Percent"/>, an amount in TL for <see cref="CouponKind.FixedAmount"/>.</summary>
	public decimal Value { get; set; }
	public decimal MinSubtotal { get; set; }
	public DateTime? ExpiresAt { get; set; }
	public int? MaxRedemptions { get; set; }
	public int RedemptionCount { get; set; }
	public bool IsActive { get; set; } = true;
}

/// <summary>One row per customer and coupon: a customer can use a coupon once.</summary>
public class CouponRedemption
{
	public int Id { get; set; }
	public int CouponId { get; set; }
	public Coupon Coupon { get; set; } = null!;
	public string UserId { get; set; } = "";
	public int OrderId { get; set; }
	public Order Order { get; set; } = null!;
	public DateTime RedeemedAt { get; set; }
}

public sealed record CouponCheck(bool IsValid, decimal Discount = 0, string? Error = null);

public static class CouponRules
{
	public const decimal MaxPercent = 90;

	public static string? NormalizeCode(string? code)
	{
		var trimmed = code?.Trim().ToUpperInvariant();
		return !string.IsNullOrEmpty(trimmed) && trimmed.Length <= 32 && trimmed.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_')
			? trimmed : null;
	}

	/// <summary>Checks a coupon definition for the admin form; returns an error message or null.</summary>
	public static string? ValidateDefinition(Coupon coupon)
	{
		if (NormalizeCode(coupon.Code) == null) return "Kupon kodu en fazla 32 karakter; harf, rakam, tire ve alt çizgi içerebilir.";
		if (coupon.Kind == CouponKind.Percent && (coupon.Value < 1 || coupon.Value > MaxPercent || coupon.Value != decimal.Truncate(coupon.Value)))
			return $"Yüzde indirim 1 ile {MaxPercent:0} arasında tam sayı olmalı.";
		if (coupon.Kind == CouponKind.FixedAmount && (coupon.Value <= 0 || decimal.Round(coupon.Value, 2) != coupon.Value))
			return "Sabit indirim 0'dan büyük olmalı ve en fazla iki ondalık içermeli.";
		if (coupon.MinSubtotal < 0) return "Alt sepet tutarı negatif olamaz.";
		if (coupon.MaxRedemptions is < 1) return "Kullanım sınırı en az 1 olmalı.";
		return null;
	}

	/// <summary>The one place that decides whether a coupon applies to a basket and by how much.</summary>
	public static CouponCheck Evaluate(Coupon? coupon, decimal subtotal, DateTime now, bool userAlreadyRedeemed)
	{
		if (coupon == null || !coupon.IsActive) return new(false, Error: "Bu kupon kodu geçerli değil.");
		if (coupon.ExpiresAt is { } expires && expires <= now) return new(false, Error: "Bu kuponun süresi dolmuş.");
		if (coupon.MaxRedemptions is { } max && coupon.RedemptionCount >= max) return new(false, Error: "Bu kuponun kullanım hakkı bitti.");
		if (userAlreadyRedeemed) return new(false, Error: "Bu kuponu daha önce kullandın.");
		if (subtotal < coupon.MinSubtotal) return new(false, Error: $"Bu kupon en az {coupon.MinSubtotal:N2} TL sepet tutarında geçerli.");
		var discount = coupon.Kind == CouponKind.Percent
			? decimal.Round(subtotal * coupon.Value / 100m, 2, MidpointRounding.AwayFromZero)
			: coupon.Value;
		// The payment provider rejects a zero total, so a coupon never takes the order below 0.01 TL.
		discount = Math.Min(discount, Math.Max(0, subtotal - 0.01m));
		return discount > 0 ? new(true, discount) : new(false, Error: "Bu kupon sepetine indirim sağlamıyor.");
	}
}
