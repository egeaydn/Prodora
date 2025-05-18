using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;

namespace Prodora.WebUI.Controllers
{
	public class ShopController : Controller
	{
		IProductServices _productServices;

		public ShopController( IProductServices productServices)
		{
			_productServices = productServices;
		}

		[Route("products/{category?}")]
		public IActionResult List(string categoryName , int page = 1)
		{
			return View();
		}
	}
}
