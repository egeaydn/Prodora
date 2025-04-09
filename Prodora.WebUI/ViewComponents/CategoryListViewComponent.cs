using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.ViewComponents
{
	public class CategoryListViewComponent : ViewComponent 
	{
		private ICategoryServices _categoryServices;

		public CategoryListViewComponent(ICategoryServices categoryServices)
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
