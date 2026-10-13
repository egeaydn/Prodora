using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Orders;
using Prodora.WebUI.Payments;

namespace Prodora.Tests;

public class OrderCancellationServiceTests
{
    private readonly IOrderServices orders = Substitute.For<IOrderServices>();
    private readonly IPaymentGateway gateway = Substitute.For<IPaymentGateway>();
    private readonly OrderCancellationService service;

    public OrderCancellationServiceTests()
    {
        gateway.IsConfigured.Returns(true);
        gateway.CancelAsync(default!, default!).ReturnsForAnyArgs(new RefundOutcome(true));
        service = new OrderCancellationService(orders, gateway, NullLogger<OrderCancellationService>.Instance);
    }

    private Order Existing(OrderStatus status, OrderPayments payment, string userId = "user-1")
    {
        var order = new Order { Id = 5, UserId = userId, OrderEnums = status, PaymentEnum = payment, PaymentId = payment == OrderPayments.CreditCard ? "pay-1" : "" };
        orders.GetOrder(5).Returns(order);
        orders.SetStatus(5, OrderStatus.Cancelled).Returns((order, status));
        return order;
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderPayments.Eft, true)]
    [InlineData(OrderStatus.Completed, OrderPayments.CreditCard, true)]
    [InlineData(OrderStatus.Completed, OrderPayments.Eft, false)]
    [InlineData(OrderStatus.Processing, OrderPayments.CreditCard, false)]
    [InlineData(OrderStatus.Shipped, OrderPayments.CreditCard, false)]
    [InlineData(OrderStatus.Cancelled, OrderPayments.CreditCard, false)]
    [InlineData(OrderStatus.PaymentUncertain, OrderPayments.CreditCard, false)]
    public void Customer_cancel_window_depends_on_status_and_payment_method(OrderStatus status, OrderPayments payment, bool expected) =>
        Assert.Equal(expected, OrderFulfillment.CustomerCanCancel(status, payment));

    [Fact]
    public async Task Unpaid_transfer_order_is_cancelled_without_touching_the_provider()
    {
        Existing(OrderStatus.Pending, OrderPayments.Eft);

        var outcome = await service.CancelAsOwnerAsync(5, "user-1", "127.0.0.1");

        Assert.Equal(CancelResult.Cancelled, outcome.Result);
        await gateway.DidNotReceiveWithAnyArgs().CancelAsync(default!, default!);
    }

    [Fact]
    public async Task Paid_card_order_is_refunded_before_it_is_marked_cancelled()
    {
        Existing(OrderStatus.Completed, OrderPayments.CreditCard);

        var outcome = await service.CancelAsOwnerAsync(5, "user-1", "10.0.0.1");

        Assert.Equal(CancelResult.Cancelled, outcome.Result);
        Assert.Equal(OrderStatus.Completed, outcome.Previous);
        Received.InOrder(() =>
        {
            gateway.CancelAsync(Arg.Any<Order>(), "10.0.0.1");
            orders.SetStatus(5, OrderStatus.Cancelled);
        });
    }

    [Fact]
    public async Task Someone_elses_order_looks_like_it_does_not_exist()
    {
        Existing(OrderStatus.Pending, OrderPayments.Eft, userId: "other-user");

        var outcome = await service.CancelAsOwnerAsync(5, "user-1", "127.0.0.1");

        Assert.Equal(CancelResult.NotFound, outcome.Result);
        orders.DidNotReceiveWithAnyArgs().SetStatus(default, default);
    }

    [Fact]
    public async Task Missing_order_is_not_found()
    {
        orders.GetOrder(9).Returns((Order?)null);

        Assert.Equal(CancelResult.NotFound, (await service.CancelAsOwnerAsync(9, "user-1", "127.0.0.1")).Result);
    }

    [Theory]
    [InlineData(OrderStatus.Processing, OrderPayments.CreditCard)]
    [InlineData(OrderStatus.Completed, OrderPayments.Eft)]
    [InlineData(OrderStatus.Cancelled, OrderPayments.CreditCard)]
    public async Task Orders_outside_the_window_are_rejected_without_any_refund(OrderStatus status, OrderPayments payment)
    {
        Existing(status, payment);

        var outcome = await service.CancelAsOwnerAsync(5, "user-1", "127.0.0.1");

        Assert.Equal(CancelResult.NotAllowed, outcome.Result);
        await gateway.DidNotReceiveWithAnyArgs().CancelAsync(default!, default!);
        orders.DidNotReceiveWithAnyArgs().SetStatus(default, default);
    }

    [Fact]
    public async Task Rejected_refund_leaves_the_order_unchanged()
    {
        Existing(OrderStatus.Completed, OrderPayments.CreditCard);
        gateway.CancelAsync(default!, default!).ReturnsForAnyArgs(new RefundOutcome(false, "refund-rejected"));

        var outcome = await service.CancelAsOwnerAsync(5, "user-1", "127.0.0.1");

        Assert.Equal(CancelResult.RefundFailed, outcome.Result);
        orders.DidNotReceiveWithAnyArgs().SetStatus(default, default);
    }

    [Fact]
    public async Task Provider_exception_leaves_the_order_unchanged()
    {
        Existing(OrderStatus.Completed, OrderPayments.CreditCard);
        gateway.CancelAsync(default!, default!).ThrowsAsyncForAnyArgs(new HttpRequestException("timeout"));

        var outcome = await service.CancelAsOwnerAsync(5, "user-1", "127.0.0.1");

        Assert.Equal(CancelResult.RefundFailed, outcome.Result);
        orders.DidNotReceiveWithAnyArgs().SetStatus(default, default);
    }

    [Fact]
    public async Task Unconfigured_gateway_cannot_cancel_a_paid_order()
    {
        Existing(OrderStatus.Completed, OrderPayments.CreditCard);
        gateway.IsConfigured.Returns(false);

        Assert.Equal(CancelResult.RefundFailed, (await service.CancelAsOwnerAsync(5, "user-1", "127.0.0.1")).Result);
        await gateway.DidNotReceiveWithAnyArgs().CancelAsync(default!, default!);
    }

    [Fact]
    public async Task Refund_followed_by_a_database_failure_is_reported_for_manual_follow_up()
    {
        Existing(OrderStatus.Completed, OrderPayments.CreditCard);
        orders.SetStatus(5, OrderStatus.Cancelled).Throws(new TimeoutException());

        var outcome = await service.CancelAsOwnerAsync(5, "user-1", "127.0.0.1");

        Assert.Equal(CancelResult.RefundedButNotUpdated, outcome.Result);
    }

    [Fact]
    public async Task Order_that_moved_on_meanwhile_is_not_cancelled()
    {
        Existing(OrderStatus.Pending, OrderPayments.Eft);
        orders.SetStatus(5, OrderStatus.Cancelled).Throws(new StoreValidationException("moved"));

        Assert.Equal(CancelResult.NotAllowed, (await service.CancelAsOwnerAsync(5, "user-1", "127.0.0.1")).Result);
    }
}
