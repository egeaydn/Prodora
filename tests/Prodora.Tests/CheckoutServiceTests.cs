using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Models;
using Prodora.WebUI.Payments;

namespace Prodora.Tests;

public class CheckoutServiceTests
{
    private readonly ICheckoutDal orders = Substitute.For<ICheckoutDal>();
    private readonly IPaymentGateway gateway = Substitute.For<IPaymentGateway>();
    private readonly CheckoutService service;

    public CheckoutServiceTests()
    {
        gateway.IsConfigured.Returns(true);
        orders.Prepare(Arg.Any<Order>()).Returns(call => new PreparedOrder(call.Arg<Order>(), true));
        service = new CheckoutService(orders, gateway, NullLogger<CheckoutService>.Instance);
    }

    private static OrderModels Form() => new()
    {
        RequestId = "request-1", Firstname = "Test", Lastname = "User", Address = "Test address",
        City = "Istanbul", Phone = "5551234567", Email = "synthetic@example.test"
    };

    [Theory]
    [InlineData("")]
    [InlineData("bitcoin")]
    [InlineData("CREDIT")]
    public async Task Unknown_payment_method_is_rejected_before_anything_is_saved(string method)
    {
        await Assert.ThrowsAsync<StoreValidationException>(() => service.SubmitAsync(Form(), "user-1", method, "127.0.0.1"));
        orders.DidNotReceiveWithAnyArgs().Prepare(default!);
    }

    [Fact]
    public async Task Card_payment_is_rejected_when_the_gateway_is_not_configured()
    {
        gateway.IsConfigured.Returns(false);
        await Assert.ThrowsAsync<StoreValidationException>(() => service.SubmitAsync(Form(), "user-1", "credit", "127.0.0.1"));
        orders.DidNotReceiveWithAnyArgs().Prepare(default!);
    }

    [Fact]
    public async Task Eft_order_is_saved_without_charging_anything()
    {
        var order = await service.SubmitAsync(Form(), "user-1", "eft", "127.0.0.1");

        Assert.Equal(OrderPayments.Eft, order.PaymentEnum);
        await gateway.DidNotReceiveWithAnyArgs().PayAsync(default!, default!, default!);
        orders.DidNotReceiveWithAnyArgs().Complete(default, default!, default, default!, default!);
    }

    [Fact]
    public async Task Repeated_request_does_not_charge_the_card_again()
    {
        var existing = new Order { Id = 7, OrderEnums = OrderStatus.Completed, PaymentEnum = OrderPayments.CreditCard };
        orders.Prepare(Arg.Any<Order>()).Returns(new PreparedOrder(existing, Created: false));

        var order = await service.SubmitAsync(Form(), "user-1", "credit", "127.0.0.1");

        Assert.Same(existing, order);
        await gateway.DidNotReceiveWithAnyArgs().PayAsync(default!, default!, default!);
        orders.DidNotReceiveWithAnyArgs().Complete(default, default!, default, default!, default!);
    }

    [Fact]
    public async Task Successful_card_payment_is_recorded_with_provider_ids()
    {
        gateway.PayAsync(Arg.Any<Order>(), Arg.Any<OrderModels>(), "10.0.0.1")
            .Returns(new PaymentOutcome(OrderStatus.Completed, "payment-9", "conversation-9"));

        var order = await service.SubmitAsync(Form(), "user-1", "credit", "10.0.0.1");

        Assert.Equal(OrderStatus.Completed, order.OrderEnums);
        Assert.Equal("user-1", order.UserId);
        Assert.Equal(OrderPayments.CreditCard, order.PaymentEnum);
        orders.Received(1).Complete(order.Id, "user-1", OrderStatus.Completed, "payment-9", "conversation-9");
    }

    [Fact]
    public async Task Failed_card_payment_is_recorded_as_failed()
    {
        gateway.PayAsync(default!, default!, default!).ReturnsForAnyArgs(new PaymentOutcome(OrderStatus.PaymentFailed));

        var order = await service.SubmitAsync(Form(), "user-1", "credit", "127.0.0.1");

        Assert.Equal(OrderStatus.PaymentFailed, order.OrderEnums);
        orders.Received(1).Complete(order.Id, "user-1", OrderStatus.PaymentFailed, "", "");
    }

    [Fact]
    public async Task Gateway_exception_leaves_the_order_for_payment_review()
    {
        gateway.PayAsync(default!, default!, default!).ThrowsAsyncForAnyArgs(new HttpRequestException("timeout"));

        var order = await service.SubmitAsync(Form(), "user-1", "credit", "127.0.0.1");

        Assert.Equal(OrderStatus.PaymentUncertain, order.OrderEnums);
        Assert.True(OrderFulfillment.NeedsPaymentReview(order.OrderEnums));
        orders.Received(1).Complete(order.Id, "user-1", OrderStatus.PaymentUncertain, "", "");
    }

    [Fact]
    public async Task Order_copies_the_customer_details_and_note()
    {
        var model = Form();
        model.OrderNote = null;

        var order = await service.SubmitAsync(model, "user-1", "eft", "127.0.0.1");

        Assert.Equal("request-1", order.RequestId);
        Assert.Equal("Test", order.FirstName);
        Assert.Equal("Istanbul", order.City);
        Assert.Equal("", order.OrderNote);
        Assert.NotEmpty(order.OrderNumber);
    }
}
