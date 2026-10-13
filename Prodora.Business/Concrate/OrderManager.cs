using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Business.Abstract;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.Business.Concrate
{
	public class OrderManager : IOrderServices
	{
		private IOrderDal _orderDal;
		public OrderManager(IOrderDal orderDal)
		{
			_orderDal = orderDal;
		}
		public void Create(Order entity)
		{
			_orderDal.Create(entity);
		}

		public List<Order> GetOrders(string userId) => _orderDal.GetOrders(userId);
		public Order? GetOrder(int orderId) => _orderDal.GetOrder(orderId);
		public List<Order> GetStoreOrders(string? bucket = null) => _orderDal.GetStoreOrders(bucket);
		public (Order Order, OrderStatus Previous) SetStatus(int orderId, OrderStatus next, string? trackingNumber = null) => _orderDal.SetStatus(orderId, next, trackingNumber);
	}
}
