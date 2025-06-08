using Prodora.Entitys;

namespace Prodora.WebUI.Models
{
	public class BasketModel
	{ 
		public int BasketId { get; set; }
		public List<BasketItemModel> BasketItems { get; set; }
		public decimal TotalPrice()
		{
			return BasketItems.Sum(item => item.Price * item.Quantity);
		}
	}

	public class BasketItemModel
	{
		public int BasketItemId { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }
		public string Image { get; set; }
	}

}
