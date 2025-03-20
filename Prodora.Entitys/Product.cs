using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Prodora.Entitys
{
	[Table("ProdoraProducts")]
	public class Product
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Ürün adı boş olamaz.")]
		[MaxLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olmalıdır.")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Açıklama boş olamaz.")]
		[MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olmalıdır.")]
		public string Description { get; set; }

		[Required(ErrorMessage = "Fiyat belirtilmelidir.")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
		[RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Fiyat en fazla iki ondalık basamak içermelidir.")]
		public decimal Price { get; set; }

		[Required(ErrorMessage = "Stok durumu belirtilmelidir.")]
		public bool Stock { get; set; }

		[MinLength(1, ErrorMessage = "En az bir kategori seçilmelidir.")]
		public List<Category> Categories { get; set; }
		public List<ProductCategory> ProductCategory { get; set; }

		[MinLength(1, ErrorMessage = "En az bir resim eklenmelidir.")]
		public List<Image> Images { get; set; }

		[Required(ErrorMessage = "Marka adı boş olamaz.")]
		[MaxLength(50, ErrorMessage = "Marka adı en fazla 50 karakter olmalıdır.")]
		public string Brand { get; set; }

		public Product()
		{
			Categories = new List<Category>();
			Images = new List<Image>();
		}
	}
}
