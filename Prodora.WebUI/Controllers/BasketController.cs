using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Models;

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

			return View(
				new BasketModel()
				{
					BasketId = basket.Id,
					BasketItems = basket.BasketItems.Select(i => new BasketItemModel()
					{
						BasketItemId = i.Id,
						ProductId = i.ProductId,
						ProductName = i.Product.Name,
						Price = i.Product.Price,
						Quantity = i.Quantity,
						Image = i.Product.Images[0].ImageUrl
					}).ToList()
				}
			);
		}

		public IActionResult AddToBasket(int productId , int quantity , string action = "addToBasket")
		{
			_basketServices.AddToBasket(_userManager.GetUserId(User),productId, quantity);
			if (action == "buyNow")//valuesini buynow yapıyoruz
			{
				return RedirectToAction("Checkout","Basket");
			}
			return View("Home");
		}
	}
}
