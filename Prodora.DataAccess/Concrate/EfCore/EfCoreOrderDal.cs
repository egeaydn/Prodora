using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore
{
	public class EfCoreOrderDal : EfCoreGenericRepository<Order, DataContext>, IOrderDal
	{
		public List<Order> GetOrders(string userId)
		{
			using (var context = new DataContext())
			{
				var orders = context.Orders.Include(o => o.OrderItems).ThenInclude(o => o.Product).ThenInclude(o => o.Images).AsQueryable();
				if (!string.IsNullOrEmpty(userId))
				{
					orders = orders.Where(o => o.UserId == userId);
				}
				return orders.ToList();
			}
		}
	}
	
}
