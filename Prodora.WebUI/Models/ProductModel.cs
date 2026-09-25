using Prodora.Entitys;
using System.ComponentModel.DataAnnotations;

namespace Prodora.WebUI.Models
{
	public class ProductModel
	{
		public int Id { get; set; }
		[Required]
		[StringLength(100, MinimumLength = 5, ErrorMessage = "Ürün adı 5–100 karakter olmalıdır.")]
		public string Name { get; set; }
		[Required]
		[StringLength(500, MinimumLength = 5, ErrorMessage = "Ürün açıklaması 5–500 karakter olmalıdır.")]
		public string Description { get; set; }
		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
		public decimal Price { get; set; }
		public List<Image> Images { get; set; }
		[Required(ErrorMessage = "Stok durumu belirtilmelidir.")]
		public bool Stock { get; set; }
		[Required]
		public string Brand { get; set; }
		public List<Category> SelectedCategories { get; set; } = new();
		public string CategoryId { get; set; }
		public ProductModel()
		{
			Images = new List<Image>();
		}
	}
}
