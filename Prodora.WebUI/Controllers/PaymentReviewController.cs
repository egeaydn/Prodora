using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Extensions;
using Prodora.WebUI.Models;
using Prodora.WebUI.Payments;
namespace Prodora.WebUI.Controllers;

[Authorize(Roles = "admin")]
public class PaymentReviewController(ICheckoutDal orders, IPaymentGateway gateway, ILogger<PaymentReviewController> logger) : Controller
{
    [HttpGet] public IActionResult Index() => View(orders.UnresolvedOrders());
    [HttpPost]
    public async Task<IActionResult> Verify(int id)
    {
        var order = orders.UnresolvedOrders().SingleOrDefault(o => o.Id == id);
        if (order == null) return RedirectToAction(nameof(Index));
        OrderStatus? confirmed = null;
        if (gateway.IsConfigured)
        {
            try
            {
                var result = await gateway.RetrieveAsync(order);
                if (result.Status is OrderStatus.Completed or OrderStatus.PaymentFailed)
                {
                    orders.Complete(order.Id, order.UserId, result.Status, result.PaymentId, result.ConversationId);
                    confirmed = result.Status;
                }
            }
            catch (Exception exception)
            {
                logger.LogWarning("Payment verification failed for order {OrderId}; error type {ErrorType}", order.Id, exception.GetType().Name);
            }
        }
        TempData.Put("message", new ResultModels
        {
            Title = "Ödeme sorgusu", Css = confirmed == OrderStatus.Completed ? "success" : "warning",
            Message = confirmed switch
            {
                OrderStatus.Completed => "iyzico test ödemesi doğrulandı; sipariş tamamlandı.",
                OrderStatus.PaymentFailed => "iyzico ödemenin başarısız olduğunu doğruladı. Sepet korundu; kullanıcı yeni bir ödeme girişimi başlatabilir.",
                _ => "Ödeme doğrulanamadı. Sipariş ve sepet korundu. iyzico sandbox panelinde siparişin ödeme referansını kontrol et; yeni tahsilat başlatılmadı."
            }
        });
        return RedirectToAction(nameof(Index));
    }
}
