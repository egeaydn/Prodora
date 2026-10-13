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
		public int? VariantId { get; set; }
		/// <summary>The variant's name when it was ordered, kept even if the variant is later renamed or switched off.</summary>
		public string? VariantName { get; set; }
		public string DisplayName => string.IsNullOrWhiteSpace(VariantName) ? (ProductName ?? "Ürün") : $"{ProductName ?? "Ürün"} · {VariantName}";
		public string? ProductImage { get; set; }
	}
}
