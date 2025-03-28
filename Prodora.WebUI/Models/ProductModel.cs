using Prodora.Entitys;
using System.ComponentModel.DataAnnotations;

namespace Prodora.WebUI.Models
{
	public class ProductModel
	{
		public int Id { get; set; }
		[Required]
		[StringLength(50, MinimumLength = 5, ErrorMessage = "Ürün adı min 5 max 50 karakter olmalıdır.")]
		public string Name { get; set; }
		[Required]
		[StringLength(200, MinimumLength = 5, ErrorMessage = "Ürün açıklaması min 5 max 200 karakter olmalıdır.")]
		public string Description { get; set; }
		[Required]
		[Range(0, double.MaxValue, ErrorMessage = "Fiyat geçerli bir değer olmalıdır. Lütfen pozitif bir sayı giriniz.")]
		public decimal Price { get; set; }
		public List<Image> Images { get; set; }

		public List<Category> SelectedCategories { get; set; }
		public string CategoryId { get; set; }
		public ProductModel()
		{
			Images = new List<Image>();
		}
	}
}
