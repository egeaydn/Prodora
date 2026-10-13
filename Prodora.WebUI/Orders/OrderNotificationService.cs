using Prodora.Entitys;
using Prodora.WebUI.EmailServices;
using Microsoft.AspNetCore.Mvc;

namespace Prodora.WebUI.Orders;

public sealed class OrderNotificationService(IAccountEmailSender email, AccountLinkBuilder links, ILogger<OrderNotificationService> logger)
{
	public Task<bool> SendPlacedAsync(Order order, IUrlHelper urls, HttpRequest request) =>
		SendAsync(order, EmailTemplates.OrderPlaced(order.FirstName, OrderRef(order),
			order.OrderEnums == OrderStatus.Pending
				? "Siparişin kaydedildi. Havale / EFT tutarı hesabımıza geçene kadar ödenmiş sayılmaz."
				: "Ödemen alındı ve siparişin hazırlığa alındı.",
			OrdersUrl(urls, request)));

	public Task<bool> SendStatusAsync(Order order, OrderStatus previous, IUrlHelper urls, HttpRequest request)
	{
		var url = OrdersUrl(urls, request);
		var name = order.FirstName;
		var reference = OrderRef(order);
		var emailBody = order.OrderEnums switch
		{
			OrderStatus.Cancelled => EmailTemplates.OrderCancelled(name, reference, url),
			OrderStatus.Shipped => EmailTemplates.OrderShipped(name, reference, url, order.TrackingNumber),
			OrderStatus.Delivered => EmailTemplates.OrderDelivered(name, reference, url),
			OrderStatus.Completed or OrderStatus.Processing when previous == OrderStatus.Pending
				=> EmailTemplates.OrderPaid(name, reference, url),
			_ => null
		};
		return emailBody == null ? Task.FromResult(true) : SendAsync(order, emailBody);
	}

	private async Task<bool> SendAsync(Order order, BrandedEmail message)
	{
		if (string.IsNullOrWhiteSpace(order.Email)) return false;
		var sent = await email.SendAsync(message, order.Email);
		if (!sent) logger.LogWarning("Order email was not delivered for order {OrderId}.", order.Id);
		return sent;
	}

	private string OrdersUrl(IUrlHelper urls, HttpRequest request) =>
		links.Create(urls, request, nameof(Controllers.BasketController.GetOrders), values: null, controller: "Basket");

	private static string OrderRef(Order order) =>
		string.IsNullOrWhiteSpace(order.OrderNumber) ? $"#{order.Id:D6}" : $"#{order.Id:D6} · {order.OrderNumber}";
}
