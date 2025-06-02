using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodora.Entitys
{
	public class Basket
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		public List<BasketItem> BasketItems { get; set; }
		
	}
	public class BasketItem 
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public Product Product { get; set; }
		public int Quantity { get; set; }
		public int BasketId { get; set; }
		public Basket Basket { get; set; }
	}
}
