using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.ViewComponents
{
	public class CategoryListViewComponents : ViewComponent
	{
		private ICategoryServices _categoryServices;
		public CategoryListViewComponents(ICategoryServices categoryServices)
		{
			_categoryServices = categoryServices;
		}

		public IViewComponentResult Invoke()
		{
			return View(
			new CategoryListViewModel()
			{
				Categories = _categoryServices.GetAll(),
				SelectedCategory = RouteData.Values["category"]?.ToString()
			}
			);
		}
	}
}
