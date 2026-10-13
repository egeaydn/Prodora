using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prodora.WebUI.Extensions;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Models;
using Prodora.WebUI.Orders;

namespace Prodora.WebUI.Controllers;

[Authorize]
public class OrdersController(UserManager<ApplicationUser> users, OrderCancellationService cancellation,
    OrderNotificationService notifications) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = users.GetUserId(User) ?? throw new InvalidOperationException("Oturum bulunamadı.");
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var outcome = await cancellation.CancelAsOwnerAsync(id, userId, ip);

        var (message, css) = outcome.Result switch
        {
            CancelResult.Cancelled => ("Siparişin iptal edildi. Kartla ödediysen iade bankana yansıyana kadar birkaç iş günü sürebilir.", "success"),
            CancelResult.NotFound => ("Bu sipariş bulunamadı.", "warning"),
            CancelResult.NotAllowed => ("Bu sipariş artık iptal edilemiyor. Havale ile ödenen veya hazırlanmaya başlayan siparişler için mağazayla iletişime geç.", "warning"),
            CancelResult.RefundFailed => ("İptal şu an tamamlanamadı; siparişin değişmedi. Biraz sonra tekrar dene.", "warning"),
            _ => ("İadeni başlattık ama siparişin durumu güncellenemedi. Sipariş numaranla mağazayla iletişime geç.", "warning")
        };
        TempData.Put("message", new ResultModels { Title = "Sipariş iptali", Message = message, Css = css });

        if (outcome.Result == CancelResult.Cancelled && outcome.Order != null)
            await notifications.SendStatusAsync(outcome.Order, outcome.Previous, Url, Request);
        return RedirectToAction("GetOrders", "Basket");
    }
}
