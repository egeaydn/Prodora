namespace Prodora.Entitys;

public static class OrderFulfillment
{
	public static IReadOnlyList<OrderStatus> NextStatuses(OrderStatus current) => current switch
	{
		OrderStatus.Pending => new[] { OrderStatus.Completed, OrderStatus.Processing, OrderStatus.Cancelled },
		OrderStatus.Completed => new[] { OrderStatus.Processing, OrderStatus.Cancelled },
		OrderStatus.Processing => new[] { OrderStatus.Shipped, OrderStatus.Cancelled },
		OrderStatus.Shipped => new[] { OrderStatus.Delivered },
		_ => Array.Empty<OrderStatus>()
	};

	public static bool CanTransition(OrderStatus from, OrderStatus to) => NextStatuses(from).Contains(to);

	public static bool NeedsPaymentReview(OrderStatus status) =>
		status is OrderStatus.PaymentPending or OrderStatus.PaymentFailed or OrderStatus.PaymentUncertain;

	// Customers may cancel before the store starts preparing the order. Bank transfers that
	// were already received are refunded by hand, so only card payments can be cancelled once paid.
	public static bool CustomerCanCancel(OrderStatus status, OrderPayments payment) =>
		status == OrderStatus.Pending || (status == OrderStatus.Completed && payment == OrderPayments.CreditCard);
}
