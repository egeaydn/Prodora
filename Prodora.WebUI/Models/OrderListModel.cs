using Prodora.Entitys;

namespace Prodora.WebUI.Models
{
	public class OrderListModel
	{
		public int OrderId { get; set; }
		public string OrderNumber { get; set; }
		public int Raiting { get; set; }
		public DateTime OrderDate { get; set; }
		public OrderStatus OrderStatusEnums{ get; set; }
		public OrderPayments OrderPamentsEnum{ get; set; }
		public string FirstName { get; set; }
		public string LastName{ get; set; }
		public string Adress{ get; set; }
		public string City{ get; set; }
		public string Phone{ get; set; }
		public string OrderNote{ get; set; }
		public List<OrderItemModel> OrderItems{ get; set; }

		public decimal TotalPrice()
		{
			return OrderItems.Sum(x => x.Price * x.Quantity);
		}

	}
	public class OrderItemModel
	{
		public int OrderItemId { get; set; }
		public string Name { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
		public string ImageUrl { get; set; }
	}
}
