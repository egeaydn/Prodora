using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.EmailServices;
using Prodora.WebUI.Orders;
using Prodora.WebUI.Payments;

namespace Prodora.Tests;

public class TrackingNumberTests
{
    [Theory]
    [InlineData("TR123456789", "TR123456789")]
    [InlineData("  tr-123 456  ", "tr-123 456")]
    public void Plain_tracking_numbers_are_trimmed_and_kept(string input, string expected) =>
        Assert.Equal(expected, OrderFulfillment.NormalizeTrackingNumber(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("TR123;DROP")]
    public void Empty_or_unsafe_tracking_numbers_are_rejected(string? input) =>
        Assert.Null(OrderFulfillment.NormalizeTrackingNumber(input));

    [Fact]
    public void Tracking_numbers_are_limited_to_forty_characters()
    {
        Assert.NotNull(OrderFulfillment.NormalizeTrackingNumber(new string('A', 40)));
        Assert.Null(OrderFulfillment.NormalizeTrackingNumber(new string('A', 41)));
    }

    [Fact]
    public void Shipped_email_includes_the_tracking_number_only_when_there_is_one()
    {
        Assert.Contains("TR999", EmailTemplates.OrderShipped("Ege", "#000001", "https://shop.test/orders", "TR999").TextBody);
        Assert.DoesNotContain("takip", EmailTemplates.OrderShipped("Ege", "#000001", "https://shop.test/orders").TextBody);
    }
}

public class AdminCancellationTests
{
    private readonly IOrderServices orders = Substitute.For<IOrderServices>();
    private readonly IPaymentGateway gateway = Substitute.For<IPaymentGateway>();
    private readonly OrderCancellationService service;

    public AdminCancellationTests()
    {
        gateway.IsConfigured.Returns(true);
        gateway.CancelAsync(default!, default!).ReturnsForAnyArgs(new RefundOutcome(true));
        service = new OrderCancellationService(orders, gateway, NullLogger<OrderCancellationService>.Instance);
    }

    private Order Existing(OrderStatus status, OrderPayments payment)
    {
        var order = new Order { Id = 8, UserId = "someone", OrderEnums = status, PaymentEnum = payment, PaymentId = payment == OrderPayments.CreditCard ? "pay-8" : "" };
        orders.GetOrder(8).Returns(order);
        orders.SetStatus(8, OrderStatus.Cancelled).Returns((order, status));
        return order;
    }

    [Fact]
    public async Task Admin_can_cancel_and_refund_an_order_that_is_already_being_prepared()
    {
        Existing(OrderStatus.Processing, OrderPayments.CreditCard);

        var outcome = await service.CancelAsAdminAsync(8, "127.0.0.1");

        Assert.Equal(CancelResult.Cancelled, outcome.Result);
        await gateway.Received(1).CancelAsync(Arg.Any<Order>(), "127.0.0.1");
    }

    [Fact]
    public async Task Customer_cannot_cancel_an_order_that_is_being_prepared_but_admin_can()
    {
        Existing(OrderStatus.Processing, OrderPayments.CreditCard);

        Assert.Equal(CancelResult.NotAllowed, (await service.CancelAsOwnerAsync(8, "someone", "127.0.0.1")).Result);
        Assert.Equal(CancelResult.Cancelled, (await service.CancelAsAdminAsync(8, "127.0.0.1")).Result);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.PaymentUncertain)]
    public async Task Admin_cannot_cancel_shipped_finished_or_unverified_orders(OrderStatus status)
    {
        Existing(status, OrderPayments.CreditCard);

        Assert.Equal(CancelResult.NotAllowed, (await service.CancelAsAdminAsync(8, "127.0.0.1")).Result);
        await gateway.DidNotReceiveWithAnyArgs().CancelAsync(default!, default!);
    }

    [Fact]
    public async Task Received_bank_transfer_is_cancelled_without_the_card_provider_and_flagged_for_manual_refund()
    {
        Existing(OrderStatus.Completed, OrderPayments.Eft);

        var outcome = await service.CancelAsAdminAsync(8, "127.0.0.1");

        Assert.Equal(CancelResult.Cancelled, outcome.Result);
        Assert.True(outcome.NeedsManualTransferRefund);
        await gateway.DidNotReceiveWithAnyArgs().CancelAsync(default!, default!);
    }

    [Fact]
    public async Task Unpaid_transfer_does_not_need_a_manual_refund()
    {
        Existing(OrderStatus.Pending, OrderPayments.Eft);

        Assert.False((await service.CancelAsAdminAsync(8, "127.0.0.1")).NeedsManualTransferRefund);
    }

    [Fact]
    public async Task Missing_order_is_not_found_for_admin_too()
    {
        orders.GetOrder(99).Returns((Order?)null);

        Assert.Equal(CancelResult.NotFound, (await service.CancelAsAdminAsync(99, "127.0.0.1")).Result);
    }
}
