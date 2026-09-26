using System.Globalization;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Options;
using Prodora.Entitys;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Payments;

public class IyzicoSettings
{
    public string ApiKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
}
public record PaymentOutcome(OrderStatus Status, string PaymentId = "", string ConversationId = "");
public interface IPaymentGateway
{
    bool IsConfigured { get; }
    Task<PaymentOutcome> PayAsync(Order order, OrderModels model, string ip);
    Task<PaymentOutcome> RetrieveAsync(Order order);
}
public class IyzicoPaymentGateway(IOptions<IyzicoSettings> settings) : IPaymentGateway
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(settings.Value.ApiKey) && !string.IsNullOrWhiteSpace(settings.Value.SecretKey);
    public async Task<PaymentOutcome> PayAsync(Order order, OrderModels model, string ip)
    {
        // Deliberately fixed to sandbox: this store is not configured for real payments.
        var options = new Iyzipay.Options { BaseUrl = "https://sandbox-api.iyzipay.com", ApiKey = settings.Value.ApiKey, SecretKey = settings.Value.SecretKey };
        var total = order.OrderItems.Sum(i => i.Price * i.Quantity).ToString("F2", CultureInfo.InvariantCulture);
        var address = new Address { ContactName = order.FirstName + " " + order.LastName, City = order.City, Country = "Türkiye", Description = order.Adress };
        var request = new CreatePaymentRequest
        {
            Locale = Locale.TR.ToString(), ConversationId = order.RequestId, Price = total, PaidPrice = total,
            Currency = Currency.TRY.ToString(), Installment = 1, BasketId = order.OrderNumber,
            PaymentChannel = PaymentChannel.WEB.ToString(), PaymentGroup = PaymentGroup.PRODUCT.ToString(),
            PaymentCard = new PaymentCard
            {
                CardHolderName = model.CardName, CardNumber = model.CardNumber?.Replace(" ", "").Replace("-", ""),
                ExpireMonth = model.ExpirationMonth, ExpireYear = model.ExpirationYear, Cvc = model.CVV, RegisterCard = 0
            },
            Buyer = new Buyer
            {
                Id = order.UserId, Name = order.FirstName, Surname = order.LastName, Email = order.Email,
                IdentityNumber = "11111111111", RegistrationAddress = order.Adress, City = order.City, Country = "Türkiye", Ip = ip
            },
            ShippingAddress = address, BillingAddress = address,
            BasketItems = order.OrderItems.Select(i => new Iyzipay.Model.BasketItem
            {
                Id = i.ProductId.ToString(), Name = i.ProductName, Category1 = "Genel",
                ItemType = BasketItemType.PHYSICAL.ToString(), Price = (i.Price * i.Quantity).ToString("F2", CultureInfo.InvariantCulture)
            }).ToList()
        };
        var payment = await Payment.Create(request, options);
        // An incomplete/mismatched reply is not proof that no payment was made.
        if (payment == null) return new(OrderStatus.PaymentUncertain);
        if (payment.Status == "failure" && string.IsNullOrEmpty(payment.PaymentId)) return new(OrderStatus.PaymentFailed);
        return Verify(payment, order);
    }

    public async Task<PaymentOutcome> RetrieveAsync(Order order)
    {
        var payment = await Payment.Retrieve(new RetrievePaymentRequest
        {
            Locale = Locale.TR.ToString(), ConversationId = order.RequestId,
            PaymentId = string.IsNullOrEmpty(order.PaymentId) ? null : order.PaymentId,
            PaymentConversationId = order.RequestId
        }, new Iyzipay.Options { BaseUrl = "https://sandbox-api.iyzipay.com", ApiKey = settings.Value.ApiKey, SecretKey = settings.Value.SecretKey });
        // A failed lookup must never unlock the basket or imply that no charge occurred.
        return payment == null ? new(OrderStatus.PaymentUncertain) : Verify(payment, order, isLookup: true);
    }

    internal static PaymentOutcome Verify(Payment payment, Order order, bool isLookup = false)
    {
        var associated = payment.ConversationId == order.RequestId && payment.BasketId == order.OrderNumber
            && (string.IsNullOrEmpty(order.PaymentId) || payment.PaymentId == order.PaymentId);
        if (isLookup && associated && payment.Status == "success"
            && !string.IsNullOrWhiteSpace(payment.PaymentId) && payment.PaymentStatus == "FAILURE")
            return new(OrderStatus.PaymentFailed, payment.PaymentId, payment.ConversationId);
        var expected = order.OrderItems.Sum(i => i.Price * i.Quantity);
        var verified = payment.Status == "success" && !string.IsNullOrWhiteSpace(payment.PaymentId)
            && payment.FraudStatus == 1
            && (!isLookup || payment.PaymentStatus == "SUCCESS")
            && associated && payment.Currency == "TRY"
            && decimal.TryParse(payment.PaidPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out var paid) && paid == expected;
        return new(verified ? OrderStatus.Completed : OrderStatus.PaymentUncertain, payment.PaymentId ?? "", payment.ConversationId ?? "");
    }
}
