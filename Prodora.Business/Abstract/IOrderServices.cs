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
	    List<Order> GetAllOrders(string userId);
		void Create(Order entity);
	}
}
