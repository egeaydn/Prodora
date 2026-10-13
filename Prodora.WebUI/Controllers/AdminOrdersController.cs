using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Extensions;
using Prodora.WebUI.Models;
using Prodora.WebUI.Audit;
using Prodora.WebUI.Orders;

namespace Prodora.WebUI.Controllers;

[Authorize(Roles = "admin")]
public class AdminOrdersController(IOrderServices orders, OrderCancellationService cancellation,
    OrderNotificationService notifications, IAuditLog audit, ILogger<AdminOrdersController> logger) : Controller
{
    private static readonly string[] Buckets = ["all", "open", "review", "closed"];

    [HttpGet]
    public IActionResult Index(string? bucket)
    {
        bucket = Buckets.Contains(bucket) ? bucket : "all";
        ViewData["Bucket"] = bucket;
        return View(orders.GetStoreOrders(bucket));
    }

    [HttpPost]
    public async Task<IActionResult> SetStatus(int id, OrderStatus next, string? trackingNumber, string? bucket)
    {
        // Cancelling goes through Cancel so a paid card order is refunded first.
        if (next == OrderStatus.Cancelled) return RedirectToAction(nameof(Index), new { bucket });
        try
        {
            var (order, previous) = orders.SetStatus(id, next, trackingNumber);
            audit.Record("order.status", $"Sipariş #{order.Id:D6}", $"{OrderStatusDisplay.Label(previous)} → {OrderStatusDisplay.Label(order.OrderEnums)}" + (next == OrderStatus.Shipped ? $", takip no {order.TrackingNumber}" : ""));
            Notice("Sipariş güncellendi", $"#{order.Id:D6} numaralı sipariş: {OrderStatusDisplay.Label(order.OrderEnums)}.", "success");
            if (!await notifications.SendStatusAsync(order, previous, Url, Request))
                logger.LogInformation("No status email was sent for order {OrderId}.", order.Id);
        }
        catch (StoreValidationException exception) { Notice("Sipariş güncellenemedi", exception.Message, "warning"); }
        return RedirectToAction(nameof(Index), new { bucket });
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id, string? bucket)
    {
        var outcome = await cancellation.CancelAsAdminAsync(id, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
        audit.Record("order.cancel", $"Sipariş #{id:D6}", $"Sonuç: {outcome.Result}" + (outcome.NeedsManualTransferRefund ? " (havale iadesi elle yapılacak)" : ""));
        var (message, css) = outcome.Result switch
        {
            CancelResult.Cancelled when outcome.NeedsManualTransferRefund =>
                ("Sipariş iptal edildi. Havale ile alınan tutar otomatik iade edilemez; müşteriye elle iade et.", "warning"),
            CancelResult.Cancelled => ("Sipariş iptal edildi; kartla ödenmişse iade başlatıldı.", "success"),
            CancelResult.NotFound => ("Sipariş bulunamadı.", "warning"),
            CancelResult.NotAllowed => ("Bu sipariş artık iptal edilemez.", "warning"),
            CancelResult.RefundFailed => ("iyzico iadeyi onaylamadı; sipariş değişmedi. Sandbox panelinden kontrol edip tekrar dene.", "warning"),
            _ => ("İade yapıldı ama sipariş durumu kaydedilemedi. Siparişi elle iptal edilmiş olarak işaretle; tekrar iade başlatma.", "warning")
        };
        Notice("Sipariş iptali", message, css);
        if (outcome.Result == CancelResult.Cancelled && outcome.Order != null)
            await notifications.SendStatusAsync(outcome.Order, outcome.Previous, Url, Request);
        return RedirectToAction(nameof(Index), new { bucket });
    }

    private void Notice(string title, string message, string css)
        => TempData.Put("message", new ResultModels { Title = title, Message = message, Css = css });
}
