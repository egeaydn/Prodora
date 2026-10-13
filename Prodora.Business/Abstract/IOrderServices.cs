using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.Business.Abstract
{
	public interface IOrderServices
	{
	    List<Order> GetOrders(string userId);
		Order? GetOrder(int orderId);
		List<Order> GetStoreOrders(string? bucket = null);
		(Order Order, OrderStatus Previous) SetStatus(int orderId, OrderStatus next, string? trackingNumber = null);
		void Create(Order entity);
	}
}
