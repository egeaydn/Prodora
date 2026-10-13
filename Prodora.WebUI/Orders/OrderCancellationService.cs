using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Payments;

namespace Prodora.WebUI.Orders;

public enum CancelResult
{
    Cancelled,
    NotFound,
    NotAllowed,
    /// <summary>The provider did not confirm the cancel/refund. The order is untouched.</summary>
    RefundFailed,
    /// <summary>The money was returned but the order could not be marked cancelled; a person must reconcile it.</summary>
    RefundedButNotUpdated
}

public sealed record CancelOutcome(CancelResult Result, Order? Order = null, OrderStatus Previous = default);

public sealed class OrderCancellationService(IOrderServices orders, IPaymentGateway gateway, ILogger<OrderCancellationService> logger)
{
    public async Task<CancelOutcome> CancelAsOwnerAsync(int orderId, string userId, string ip)
    {
        var order = orders.GetOrder(orderId);
        if (order == null || order.UserId != userId) return new(CancelResult.NotFound);
        if (!OrderFulfillment.CustomerCanCancel(order.OrderEnums, order.PaymentEnum)) return new(CancelResult.NotAllowed);

        var paidByCard = order.OrderEnums == OrderStatus.Completed;
        if (paidByCard)
        {
            if (!gateway.IsConfigured) return new(CancelResult.RefundFailed);
            RefundOutcome refund;
            try { refund = await gateway.CancelAsync(order, ip); }
            catch (Exception exception)
            {
                // The outcome is unknown, so leave the order as it is and never start a second refund here.
                logger.LogWarning("Cancel outcome unknown for order {OrderId}; error type {ErrorType}", order.Id, exception.GetType().Name);
                return new(CancelResult.RefundFailed);
            }
            if (!refund.Succeeded)
            {
                logger.LogWarning("Provider did not confirm the refund for order {OrderId}: {Detail}", order.Id, refund.Detail);
                return new(CancelResult.RefundFailed);
            }
        }

        try
        {
            var (updated, previous) = orders.SetStatus(order.Id, OrderStatus.Cancelled);
            return new(CancelResult.Cancelled, updated, previous);
        }
        catch (Exception exception) when (paidByCard)
        {
            logger.LogCritical("Order {OrderId} was refunded but could not be marked cancelled; error type {ErrorType}", order.Id, exception.GetType().Name);
            return new(CancelResult.RefundedButNotUpdated, order);
        }
        catch (StoreValidationException)
        {
            // Another request already moved the order; it is no longer cancellable by the customer.
            return new(CancelResult.NotAllowed);
        }
    }
}
