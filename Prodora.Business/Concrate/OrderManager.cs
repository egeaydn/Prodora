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

		public List<Order> GetOrders(string userId)
		{
			return _orderDal.GetOrders(userId);
		}
	}
}
