using Prodora.Entitys;

namespace Prodora.WebUI.Models
{
	public class ProductDetail
	{
		public Product Products { get; set; }
		public List<Category> Categories { get; set; }
		public List<Comment> Comments { get; set; }
		public List<Product> RelatedProducts { get; set; } // Yeni alan
	}
}
