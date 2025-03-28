using Prodora.Entitys;

namespace Prodora.WebUI.Models
{
	public class CategoryListViewModel
	{
		public string SelectedCategory { get; set; }
		public List<Category> Categories { get; set; }
	}
}
