using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Prodora.Entitys
{
	public class Order
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		public string OrderNumber { get; set; }
		public DateTime OrderDate { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Adress { get; set; }
		public string City { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
		public string OrderNote { get; set; }
		public string PaymentId { get; set; }
		public string PaymentToken { get; set; }
		public string ConversionId { get; set; }
		public OrderStatus OrderEnums { get; set; }
		public OrderPayments PaymentEnum { get; set; }
		public List<OrderItem> OrderItems { get; set; }
		public Order()
		{
			OrderItems = new List<OrderItem>();
		}
	}
	public enum OrderStatus
	{
		Pending = 0,
		Processing = 1,
		Shipped = 2,
		Delivered = 3,
		Cancelled = 4,
		Completed = 5
	}
	public enum OrderPayments
	{
		// Ödeme türleri için örnekler
		None = 0,
		CashOnDelivery = 1,
		PayPal = 2,
		CreditCard = 3,
		BankTransfer = 4,
		Stripe = 5,
		ApplePay = 6,
		GooglePay = 7,
		Bitcoin = 8,
		Other = 9,
		Eft = 10
	}
}
