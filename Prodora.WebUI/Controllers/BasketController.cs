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
    CheckoutService checkout, ICouponServices coupons, IAddressServices addresses, ILogger<BasketController> logger) : Controller
{
    private string UserId => users.GetUserId(User) ?? throw new InvalidOperationException("Oturum bulunamadı.");
    public IActionResult Home() => View(BasketView());

    [HttpPost]
    public IActionResult AddToBasket(int productId, int quantity, string action = "addToBasket", int? variantId = null)
    {
        try { baskets.AddToBasket(UserId, productId, quantity, variantId); }
        catch (StoreValidationException exception)
        {
            Notice("Sepet güncellenemedi", exception.Message, "warning");
            // Ask for the missing choice on the product page instead of the basket.
            return exception.Message.StartsWith("Önce bir seçenek") ? RedirectToAction("Details", "Shop", new { id = productId }) : RedirectToAction(nameof(Home));
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
    [HttpPost]
    public IActionResult RemoveItem(int basketItemId)
    {
        try { baskets.RemoveItem(UserId, basketItemId); }
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
    public IActionResult Checkout(string? coupon, int? address)
    {
        var model = new OrderModels { RequestId = Guid.NewGuid().ToString("N"), BasketTemplate = BasketView(), CouponCode = coupon };
        ShowCoupon(model);
        model.SavedAddresses = addresses.GetAll(UserId);
        // A chosen address wins; otherwise the default one is filled in so a returning customer can pay straight away.
        var chosen = address != null ? model.SavedAddresses.FirstOrDefault(a => a.Id == address) : model.SavedAddresses.FirstOrDefault(a => a.IsDefault);
        if (chosen != null)
        {
            model.Firstname = chosen.FirstName; model.Lastname = chosen.LastName; model.Phone = chosen.Phone;
            model.Address = chosen.Address; model.City = chosen.City;
        }
        return View(model);
    }

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
                if (model.SaveAddress) SaveAddressBestEffort(model);
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
        ShowCoupon(model);
        model.SavedAddresses = addresses.GetAll(UserId);
        model.CardNumber = null;
        model.CVV = null;
        ModelState.Remove("CardNumber");
        ModelState.Remove("CVV");
        return View(model);
    }

    public IActionResult GetOrders() => View(orders.GetOrders(UserId).Select(order => new OrderListModel
    {
        OrderId = order.Id, Adress = order.Adress, OrderNumber = order.OrderNumber, OrderDate = order.OrderDate,
        OrderStatusEnums = order.OrderEnums, TrackingNumber = order.TrackingNumber, CouponCode = order.CouponCode, DiscountAmount = order.DiscountAmount, OrderPamentsEnum = order.PaymentEnum, OrderNote = order.OrderNote,
        City = order.City, Email = order.Email, FirstName = order.FirstName, LastName = order.LastName, Phone = order.Phone,
        OrderItems = order.OrderItems.Select(i => new OrderItemModel
        {
            OrderItemId = i.Id, Name = i.DisplayName, Price = i.Price, Quantity = i.Quantity,
            ImageUrl = i.ProductImage ?? i.Product?.Images?.FirstOrDefault()?.ImageUrl ?? "product-placeholder.svg"
        }).ToList()
    }).ToList());

    // The order is already placed at this point, so a failure to remember the address must never surface as a checkout error.
    private void SaveAddressBestEffort(OrderModels model)
    {
        try
        {
            addresses.Add(UserId, new SavedAddress
            {
                Title = string.IsNullOrWhiteSpace(model.City) ? "Adres" : $"{model.City.Trim()} adresi",
                FirstName = model.Firstname, LastName = model.Lastname, Phone = model.Phone, Address = model.Address, City = model.City
            });
        }
        catch (Exception exception) { logger.LogInformation("Address was not saved to the address book; error type {ErrorType}", exception.GetType().Name); }
    }

    // Previews the coupon for the summary. The real discount is decided again, inside the checkout transaction.
    private void ShowCoupon(OrderModels model)
    {
        if (string.IsNullOrWhiteSpace(model.CouponCode)) { model.CouponCode = null; return; }
        var check = coupons.Preview(model.CouponCode, UserId, model.BasketTemplate!.TotalPrice());
        model.CouponApplied = check.IsValid;
        model.CouponDiscount = check.IsValid ? check.Discount : 0;
        model.CouponCode = check.IsValid ? CouponRules.NormalizeCode(model.CouponCode) : null;
        model.CouponMessage = check.IsValid ? "Kupon uygulandı." : check.Error;
    }

    private BasketModel BasketView()
    {
        var basket = baskets.GetBasketByUserId(UserId);
        return new BasketModel
        {
            BasketId = basket?.Id ?? 0,
            BasketItems = basket?.BasketItems.Select(i => new BasketItemModel
            {
                BasketItemId = i.Id, ProductId = i.ProductId, ProductName = i.Product?.Name ?? "Ürün", VariantName = i.Variant?.Name,
                Price = i.Product?.Price ?? 0, Quantity = i.Quantity,
                IsAvailable = i.Product is { Stock: true, IsArchived: false } && i.Product.Price > 0 && i.Quantity is >= 1 and <= 99
                    && (i.Product.StockQuantity == null || i.Quantity <= i.Product.StockQuantity)
                    && (i.Variant == null || (i.Variant.IsActive && i.Quantity <= i.Variant.StockQuantity)),
                Image = i.Product?.Images?.FirstOrDefault()?.ImageUrl ?? "product-placeholder.svg"
            }).ToList() ?? new()
        };
    }
    private void Notice(string title, string message, string css)
        => TempData.Put("message", new ResultModels { Title = title, Message = message, Css = css });
}
