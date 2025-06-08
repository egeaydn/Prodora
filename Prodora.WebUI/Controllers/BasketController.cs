using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.WebUI.Identity;

namespace Prodora.WebUI.Controllers
{
	public class BasketController : Controller
	{
		private IBasketServices _basketServices;
		private IProductServices _productServices;
		private IOrderServices _orderServices;
		private UserManager<ApplicationUser> _userManager;
		public BasketController(IBasketServices basketServices , IProductServices productServices , IOrderServices orderServices , UserManager<ApplicationUser> userManager)
		{
			_basketServices = basketServices;
			_productServices = productServices;
			_orderServices = orderServices;
			_userManager = userManager;
		}
		public IActionResult Home()
		{
			var basket = _basketServices.GetBasketByUserId(_userManager.GetUserId(User));

			return View();
		}
	}
}
