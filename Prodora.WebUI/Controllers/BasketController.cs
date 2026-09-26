using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Extensions;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Models;
using Prodora.WebUI.Payments;

namespace Prodora.WebUI.Controllers;
[Authorize]
public class BasketController(IBasketServices baskets, IOrderServices orders, UserManager<ApplicationUser> users,
    CheckoutService checkout, ILogger<BasketController> logger) : Controller
{
    private string UserId => users.GetUserId(User) ?? throw new InvalidOperationException("Oturum bulunamadı.");
    public IActionResult Home() => View(BasketView());

    [HttpPost]
    public IActionResult AddToBasket(int productId, int quantity, string action = "addToBasket")
    {
        try { baskets.AddToBasket(UserId, productId, quantity); }
        catch (StoreValidationException exception)
        {
            Notice("Sepet güncellenemedi", exception.Message, "warning");
            return RedirectToAction(nameof(Home));
        }
        return RedirectToAction(action == "buyNow" ? nameof(Checkout) : nameof(Home));
    }
    [HttpPost]
    public IActionResult DeleteFromBasket(int productId)
    {
        try { baskets.DeleteFromBasket(UserId, productId); }
        catch (StoreValidationException exception) { Notice("Sepet güncellenemedi", exception.Message, "warning"); }
        return RedirectToAction(nameof(Home));
    }
    [AllowAnonymous, HttpGet]
    public IActionResult GetBasketItemCount()
    {
        var userId = users.GetUserId(User);
        return Json(userId == null ? 0 : baskets.GetBasketByUserId(userId)?.BasketItems.Sum(i => i.Quantity) ?? 0);
    }
    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Checkout() => View(new OrderModels { RequestId = Guid.NewGuid().ToString("N"), BasketTemplate = BasketView() });

    [HttpPost, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Checkout(OrderModels model, string paymentMethod)
    {
        ViewData["PaymentMethod"] = paymentMethod;
        if (paymentMethod is not ("credit" or "eft")) ModelState.AddModelError("", "Geçerli bir ödeme yöntemi seç.");
        if (paymentMethod == "eft")
            foreach (var field in new[] { "CardName", "CardNumber", "CVV", "ExpirationMonth", "ExpirationYear" }) ModelState.Remove(field);
        if (!Guid.TryParseExact(model.RequestId, "N", out _)) ModelState.AddModelError("", "Ödeme sayfasını yenileyip tekrar dene.");
        if (ModelState.IsValid)
        {
            try
            {
                var order = await checkout.SubmitAsync(model, UserId, paymentMethod, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
                var message = order.OrderEnums switch
                {
                    OrderStatus.Completed => "Test ödemen tamamlandı ve siparişin kaydedildi.",
                    OrderStatus.Pending => "Test siparişin kaydedildi. Havale / EFT henüz ödenmiş sayılmıyor.",
                    OrderStatus.PaymentFailed => "Test ödemen onaylanmadı. Sepetin korundu; ödeme sayfasından yeniden deneyebilirsin.",
                    _ => "Ödeme girişiminin sonucu henüz doğrulanamadı. Yeniden ödeme başlatılmadı; sipariş numaranla mağaza yöneticisine başvur."
                };
                Notice("Sipariş durumu", message, order.OrderEnums is OrderStatus.Completed or OrderStatus.Pending ? "success" : "warning");
                return RedirectToAction(nameof(GetOrders));
            }
            catch (StoreValidationException exception) { ModelState.AddModelError("", exception.Message); }
            catch (Exception exception)
            {
                logger.LogError("Checkout could not finish; error type {ErrorType}", exception.GetType().Name);
                Notice("İşlem tamamlanamadı", "Siparişlerim sayfasından son işleminin durumunu kontrol et. Sonuç doğrulanmadan yeniden ödeme başlatma.", "warning");
                return RedirectToAction(nameof(GetOrders));
            }
        }
        model.BasketTemplate = BasketView();
        model.CardNumber = null;
        model.CVV = null;
        ModelState.Remove("CardNumber");
        ModelState.Remove("CVV");
        return View(model);
    }

    public IActionResult GetOrders() => View(orders.GetOrders(UserId).Select(order => new OrderListModel
    {
        OrderId = order.Id, Adress = order.Adress, OrderNumber = order.OrderNumber, OrderDate = order.OrderDate,
        OrderStatusEnums = order.OrderEnums, OrderPamentsEnum = order.PaymentEnum, OrderNote = order.OrderNote,
        City = order.City, Email = order.Email, FirstName = order.FirstName, LastName = order.LastName, Phone = order.Phone,
        OrderItems = order.OrderItems.Select(i => new OrderItemModel
        {
            OrderItemId = i.Id, Name = i.ProductName ?? i.Product?.Name ?? "Ürün", Price = i.Price, Quantity = i.Quantity,
            ImageUrl = i.ProductImage ?? i.Product?.Images?.FirstOrDefault()?.ImageUrl ?? "product-placeholder.svg"
        }).ToList()
    }).ToList());

    private BasketModel BasketView()
    {
        var basket = baskets.GetBasketByUserId(UserId);
        return new BasketModel
        {
            BasketId = basket?.Id ?? 0,
            BasketItems = basket?.BasketItems.Select(i => new BasketItemModel
            {
                BasketItemId = i.Id, ProductId = i.ProductId, ProductName = i.Product?.Name ?? "Ürün",
                Price = i.Product?.Price ?? 0, Quantity = i.Quantity,
                IsAvailable = i.Product is { Stock: true, IsArchived: false } && i.Product.Price > 0 && i.Quantity is >= 1 and <= 99,
                Image = i.Product?.Images?.FirstOrDefault()?.ImageUrl ?? "product-placeholder.svg"
            }).ToList() ?? new()
        };
    }
    private void Notice(string title, string message, string css)
        => TempData.Put("message", new ResultModels { Title = title, Message = message, Css = css });
}
