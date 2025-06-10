using System.Globalization;
using System.Net;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
			return RedirectToAction("Home");
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
						TempData.Put("message", new ResultModels()
						{
							Title = "Sipariş Başarısız",
							Message = "Siparişiniz alınırken bir hata oluştu. Lütfen tekrar deneyin.",
							Css = "danger"
						});
					}

				}
				else
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
					return View(orderModels);

			}

			return View();
		}

		public async Task<Payment> PaymentProccess (OrderModels model)
		{

			Options options = new Options()
			{
				BaseUrl = "https://sandbox-api.iyzipay.com", // Test ortamı API URL'si
				ApiKey = "sandbox-cNnJEaoyNt0sCREL4nOq8PajTLQwWeXz", // Test ortamı API anahtarı
				SecretKey = "sandbox-cmJxJfaGlVarqNV3c5ZQcMTwVNh8qswx" // Test ortamı gizli anahtar
			};

			string extarnalIpString = new WebClient().DownloadString("http://icanhazip.org/").Replace("\\r\\n", "").Replace("\\n","").Trim();

			var extarnalIp = IPAddress.Parse(extarnalIpString);

			CreatePaymentRequest request = new CreatePaymentRequest();
			request.Locale = Locale.TR.ToString(); // Dil ayarını Türkçe olarak ayarlıyoruz
			request.ConversationId = Guid.NewGuid().ToString(); // Her ödeme için benzersiz bir konuşma ID'si oluşturuyoruz
			request.Price = model.BasketTemplate.TotalPrice().ToString("F2", CultureInfo.InvariantCulture);// Ödeme tutarını ayarlıyoruz (örnek olarak 100 TL)
			request.PaidPrice = model.BasketTemplate.TotalPrice().ToString("F2", CultureInfo.InvariantCulture);
			request.Currency = Currency.TRY.ToString(); // Para birimini Türk Lirası olarak ayarlıyoruz
			request.Installment = 1; // Taksit sayısını 1 olarak ayarlıyoruz (tek çekim)
			request.BasketId = model.BasketTemplate.BasketId.ToString(); // Sepet ID'sini ayarlıyoruz
			request.PaymentChannel = PaymentChannel.WEB.ToString(); // Ödeme kanalını web olarak ayarlıyoruz
			request.PaymentGroup = PaymentGroup.PRODUCT.ToString(); // Ödeme grubunu ürün olarak ayarlıyoruz

			PaymentCard paymentCard = new PaymentCard()
			{
				CardHolderName = model.CardName, // Kart üzerindeki ismi alıyoruz
				CardNumber = model.CardNumber, // Kart numarasını alıyoruz
				ExpireMonth = model.ExpirationMonth, // Kartın son kullanma ayını alıyoruz
				ExpireYear = model.ExpirationYear, // Kartın son kullanma yılını alıyoruz
				Cvc = model.CVV, // Kartın CVV kodunu alıyoruz
				RegisterCard = 0 // Kayıtlı kart ID'si yoksa null bırakıyoruz
			};

			request.PaymentCard = paymentCard; // PaymentCard nesnesini request'e ekliyoruz

			Buyer buyer = new Buyer()
			{
				Id = _userManager.GetUserId(User), // Kullanıcı ID'sini alıyoruz
				Name = model.Firstname, // Adı alıyoruz
				Surname = model.Lastname, // Soyadı alıyoruz
				Email = model.Email, // E-posta adresini alıyoruz
				IdentityNumber = "11111111111", // Kimlik numarasını alıyoruz (örnek olarak 11 haneli bir sayı)
				RegistrationAddress = model.Address, // Kayıt adresini alıyoruz
				City = model.City, // Şehri alıyoruz
				ZipCode = "34000", // Posta kodunu alıyoruz (örnek olarak 5 haneli bir sayı)
				Ip = extarnalIp.ToString(), // Kullanıcının IP adresini alıyoruz
				Country = "Türkiye", // Ülke bilgisini alıyoruz
			};

			Address address = new Address()
			{
				ContactName = $"{model.Firstname} {model.Lastname}", // Ad ve soyadı birleştiriyoruz
				City = model.City, // Şehri alıyoruz
				Country = "Türkiye", // Ülke bilgisini alıyoruz
				ZipCode = "34000", // Posta kodunu alıyoruz (örnek olarak 5 haneli bir sayı)
				Description = model.Address // Açıklama olarak adresi alıyoruz
			};

			request.ShippingAddress = address; // ShippingAddress(fatura adresi) olarak adresi ekliyoruz
			request.BillingAddress = address; // BillingAddress(teslimat adresi) olarak aynı adresi ekliyoruz

			List<Iyzipay.Model.BasketItem> basketItems = new List<Iyzipay.Model.BasketItem>();
			Iyzipay.Model.BasketItem basketItem;

			foreach (var basketıtem in model.BasketTemplate.BasketItems)
			{ 
				basketItem = new Iyzipay.Model.BasketItem()
				{
					Id = basketıtem.BasketItemId.ToString(), // Sepet öğesi ID'sini alıyoruz
					Name =basketıtem.ProductName, // Ürün adını alıyoruz
					Category1 = _productServices.GetProductDetail(basketıtem.ProductId).ProductCategory.FirstOrDefault().ToString(), // Kategori 1 olarak genel bir kategori belirliyoruz
					ItemType = BasketItemType.PHYSICAL.ToString(), // Ürün tipini fiziksel olarak ayarlıyoruz
					Price = (basketıtem.Price * basketıtem.Quantity).ToString().Split(',')[0] // Ürün fiyatını ve miktarını çarpıyoruz
				};
				basketItems.Add(basketItem); // Sepet öğesini listeye ekliyoruz
			}

			request.BasketItems = basketItems; // Sepet öğelerini request'e ekliyoruz

			Payment payment = await Payment.Create(request,options); // Ödeme işlemini başlatıyoruz

			return payment;
		}

		public void SaveOrder(OrderModels model, string userId)
		{
			Order order = new Order()
			{
				OrderNumber = Guid.NewGuid().ToString(),
				OrderDate = DateTime.Now,
				OrderEnums = OrderStatus.Completed,
				PaymentEnum = OrderPayments.Eft, // Ödeme türünü Eft olarak ayarlıyoruz
				FirstName = model.Firstname,
				LastName = model.Lastname,
				Adress = model.Address,
				City = model.City,
				Phone = model.Phone,
				Email = model.Email,
				OrderNote = model.OrderNote,
				UserId = userId,
				PaymentToken = Guid.NewGuid().ToString(),
				ConversionId = Guid.NewGuid().ToString(),
				PaymentId = Guid.NewGuid().ToString(),
			};

			foreach (var basketItem in model.BasketTemplate.BasketItems)
			{
				var orderItem = new Entitys.OrderItem()
				{
					Price = basketItem.Price,
					Quantity = basketItem.Quantity,
					ProductId = basketItem.ProductId
				};
				order.OrderItems.Add(orderItem);
			}

			_orderServices.Create(order);

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
