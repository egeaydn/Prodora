using Prodora.DataAccess.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Models;
namespace Prodora.WebUI.Payments;

public class CheckoutService(ICheckoutDal orders, IPaymentGateway gateway, ILogger<CheckoutService> logger)
{
    public async Task<Order> SubmitAsync(OrderModels model, string userId, string method, string ip)
    {
        if (method is not ("credit" or "eft")) throw new StoreValidationException("Geçerli bir ödeme yöntemi seç.");
        if (method == "credit" && !gateway.IsConfigured) throw new StoreValidationException("Test ödeme hizmeti henüz yapılandırılmadı.");
        var prepared = orders.Prepare(new Order
        {
            RequestId = model.RequestId, UserId = userId, OrderNumber = Guid.NewGuid().ToString("N"), OrderDate = DateTime.Now,
            FirstName = model.Firstname, LastName = model.Lastname, Adress = model.Address, City = model.City,
            Phone = model.Phone, Email = model.Email, OrderNote = model.OrderNote ?? "",
            PaymentId = "", PaymentToken = "", ConversionId = "",
            PaymentEnum = method == "credit" ? OrderPayments.CreditCard : OrderPayments.Eft
        });
        var order = prepared.Order;
        if (!prepared.Created || order.PaymentEnum == OrderPayments.Eft) return order;
        PaymentOutcome outcome;
        try { outcome = await gateway.PayAsync(order, model, ip); }
        catch (Exception exception)
        {
            // Never log card values, provider request/response bodies or credentials.
            logger.LogWarning("Payment outcome unknown for order {OrderId}; error type {ErrorType}", order.Id, exception.GetType().Name);
            outcome = new(OrderStatus.PaymentUncertain);
        }
        // If this commit fails, the durable PaymentPending record still blocks another charge.
        orders.Complete(order.Id, userId, outcome.Status, outcome.PaymentId, outcome.ConversationId);
        order.OrderEnums = outcome.Status;
        return order;
    }
}
