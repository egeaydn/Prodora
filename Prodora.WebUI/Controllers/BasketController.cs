using Iyzipay.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Extensions;
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
		public BasketController(IBasketServices basketServices, IProductServices productServices, IOrderServices orderServices, UserManager<ApplicationUser> userManager)
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

		public void ClearBasket(string id)
		{
			_basketServices.ClearBasket(id);
		}

		public IActionResult AddToBasket(int productId, int quantity, string action = "addToBasket")
		{
			_basketServices.AddToBasket(_userManager.GetUserId(User), productId, quantity);
			if (action == "buyNow")//valuesini buynow yapıyoruz
			{
				return RedirectToAction("Checkout", "Basket");
			}
			return View("Home");
		}

		[HttpPost]
		public IActionResult DeleteFromBasket(int productId)
		{
			_basketServices.DeleteFromBasket(_userManager.GetUserId(User), productId);
			return RedirectToAction("Home");
		}

		public IActionResult GetBasketItemCount()
		{
			var userId = _userManager.GetUserId(User);
			if (userId == null)
			{
				return Json(0);
			}

			var basket = _basketServices.GetBasketByUserId(userId);
			int totalItemCount = basket?.BasketItems?.Sum(i => i.Quantity) ?? 0;

			return Json(totalItemCount);
		}

		public IActionResult Checkout(BasketModel? orderModel)
		{
			var basket = _basketServices.GetBasketByUserId(_userManager.GetUserId(User));
			var basketModel = new BasketModel() // Renamed from 'orderModel' to 'basketModel' to avoid conflict
			{
				BasketId = basket.Id,
				BasketItems = basket.BasketItems.Select(item => new BasketItemModel
				{
					BasketItemId = item.Id,
					ProductId = item.ProductId,
					ProductName = item.Product.Name,
					Price = item.Product.Price,
					Quantity = item.Quantity,
					Image = item.Product.Images[0].ImageUrl
				}).ToList()
			};

			return View(basketModel); // Pass the correct model to the view
		}

		[HttpPost]
		public IActionResult Checkout(OrderModels orderModels , string paymentMethod)
		{
			ModelState.Remove("BasketTemplate"); // Remove BasketModel from validation since it's not needed here

			if (ModelState.IsValid)
			{
				var userId = _userManager.GetUserId(User);
				var basket = _basketServices.GetBasketByUserId(userId);

				orderModels.BasketTemplate = new BasketModel()
				{
					BasketId = basket.Id,
					BasketItems = basket.BasketItems.Select(item => new BasketItemModel
					{
						BasketItemId = item.Id,
						ProductId = item.ProductId,
						ProductName = item.Product.Name,
						Price = item.Product.Price,
						Quantity = item.Quantity,
						Image = item.Product.Images[0].ImageUrl
					}).ToList()
				};

				if (paymentMethod == "credit")
				{
					var paymet = PaymentProccess(orderModels);

					if (paymet.Result.Status == "success")
					{
						SaveOrder(orderModels, userId);
						ClearBasket(basket.Id.ToString());
						TempData.Put("message", new ResultModels()
						{
							Title = "Sipariş Başarılı",
							Message = "Siparişiniz başarıyla alınmıştır.",
							Css = "success"
						});
					}
					else
					{
						
					}

				}

			}

			return View();
		}

		public async Task<IActionResult> PaymentProccess ()
		{
			return View();
		}

		public void SaveOrder()
		{

		}
		public IActionResult GetOrders()
		{
			var userId = _userManager.GetUserId(User);
			var orders = _orderServices.GetOrders(userId) ?? new List<Order>(); // Null kontrolü eklendi

			var orderListModel = new List<OrderListModel>();

			foreach (var order in orders)
			{
				var orderModel = new OrderListModel
				{
					OrderId = order.Id,
					Adress = order.Adress,
					OrderNumber = order.OrderNumber,
					OrderDate = order.OrderDate,
					OrderStatusEnums = order.OrderEnums,
					OrderPamentsEnum = order.PaymentEnum,
					OrderNote = order.OrderNote,
					City = order.City,
					Email = order.Email,
					FirstName = order.FirstName,
					LastName = order.LastName,
					Phone = order.Phone,
					OrderItems = order.OrderItems.Select(i => new OrderItemModel()
					{
						OrderItemId = i.Id,
						Name = i.Product.Name,
						Price = i.Product.Price,
						Quantity = i.Quantity,
						ImageUrl = i.Product.Images[0].ImageUrl
					}).ToList()
				};

				orderListModel.Add(orderModel);
			}

			return View(orderListModel);
		}
	
	}
}
// Hata CS0136: 'orderModel' adlı bir yerel veya parametre, bu ad bir kapanış yerel kapsamında bir yereli veya parametreyi tanımlamak için kullanıldığından bu kapsamda ifade edilemiyor
// Açıklama: Bir metot parametresi ile aynı isimde bir yerel değişken tanımlanmış. Bu, C# dilinde geçersizdir. Parametre ile aynı isimde bir değişken tanımlayamazsınız. 
// Çözüm: Yerel değişkenin adını değiştirin (örneğin 'basketModel' olarak)
