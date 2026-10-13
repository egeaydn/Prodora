using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodora.Entitys
{
	public class OrderItem
	{
		public int Id { get; set; }
		public int OrderId { get; set; }
		public Order Order { get; set; }
		public int ProductId { get; set; }
		public Product Product { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
		/// <summary>Units held from stock for this item while the order is open. Zero when nothing was held or it was given back.</summary>
		public int StockReserved { get; set; }
		public string? ProductName { get; set; }
		public string? ProductImage { get; set; }
	}
}
