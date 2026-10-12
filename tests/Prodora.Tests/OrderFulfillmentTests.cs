using Prodora.Entitys;

namespace Prodora.Tests;

public class OrderFulfillmentTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Completed)]
    [InlineData(OrderStatus.Pending, OrderStatus.Processing)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Completed, OrderStatus.Processing)]
    [InlineData(OrderStatus.Completed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Processing, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Processing, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
    public void Allowed_transitions_are_accepted(OrderStatus from, OrderStatus to) =>
        Assert.True(OrderFulfillment.CanTransition(from, to));

    [Theory]
    [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Processing)]
    [InlineData(OrderStatus.Processing, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Pending, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered)]
    public void Skipped_or_backwards_transitions_are_rejected(OrderStatus from, OrderStatus to) =>
        Assert.False(OrderFulfillment.CanTransition(from, to));

    [Theory]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Finished_orders_cannot_move(OrderStatus status) =>
        Assert.Empty(OrderFulfillment.NextStatuses(status));

    [Theory]
    [InlineData(OrderStatus.PaymentPending)]
    [InlineData(OrderStatus.PaymentFailed)]
    [InlineData(OrderStatus.PaymentUncertain)]
    public void Unresolved_payments_cannot_be_fulfilled_and_need_review(OrderStatus status)
    {
        Assert.True(OrderFulfillment.NeedsPaymentReview(status));
        Assert.Empty(OrderFulfillment.NextStatuses(status));
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Shipped)]
    public void Settled_orders_do_not_need_payment_review(OrderStatus status) =>
        Assert.False(OrderFulfillment.NeedsPaymentReview(status));

    [Fact]
    public void No_status_can_transition_to_itself()
    {
        foreach (var status in Enum.GetValues<OrderStatus>())
            Assert.False(OrderFulfillment.CanTransition(status, status), status.ToString());
    }
}
