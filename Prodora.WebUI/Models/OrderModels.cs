using System.ComponentModel.DataAnnotations;

namespace Prodora.WebUI.Models
{
	public class OrderModels // Bu Modeli Ödeme İşlemini Yapmak İçin Kullanacağız
	{
		[Required(ErrorMessage = "Ad alanı boş bırakılamaz.")]
		[MaxLength(50, ErrorMessage = "Ad en fazla 50 karakter olmalıdır.")]
		public string Firstname { get; set; }

		[Required(ErrorMessage = "Soyad alanı boş bırakılamaz.")]
		[MaxLength(50, ErrorMessage = "Soyad en fazla 50 karakter olmalıdır.")]
		public string Lastname { get; set; }

		[Required(ErrorMessage = "Adres alanı boş bırakılamaz.")]
		[MaxLength(250, ErrorMessage = "Adres en fazla 250 karakter olmalıdır.")]
		public string Address { get; set; }

		[Required(ErrorMessage = "Şehir alanı boş bırakılamaz.")]
		public string City { get; set; }

		[Required(ErrorMessage = "Telefon alanı boş bırakılamaz.")]
		[Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
		public string Phone { get; set; }

		[Required(ErrorMessage = "Email alanı boş bırakılamaz.")]
		[EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Kart üzerindeki isim boş bırakılamaz.")]
		public string? CardName { get; set; }

		[Required(ErrorMessage = "Kart numarası boş bırakılamaz.")]
		public string? CardNumber { get; set; }

		[Required(ErrorMessage = "CVV boş bırakılamaz.")]
		[StringLength(4, MinimumLength = 3, ErrorMessage = "CVV 3 veya 4 haneli olmalıdır.")]
		public string? CVV { get; set; }

		[Required(ErrorMessage = "Ay bilgisi zorunludur.")]
		[RegularExpression("^(0[1-9]|1[0-2])$", ErrorMessage = "Geçerli bir ay giriniz (01-12).")]
		public string? ExpirationMonth { get; set; }

		[Required(ErrorMessage = "Yıl bilgisi zorunludur.")]
		[RegularExpression("^20[2-9][0-9]$", ErrorMessage = "Geçerli bir yıl giriniz (2025 ve sonrası).")]
		public string? ExpirationYear { get; set; }

		[MaxLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir.")]
		public string? OrderNote { get; set; }
		public BasketModel BasketTemplate { get; set; } // Sepet bilgilerini tutan model
	}
}
