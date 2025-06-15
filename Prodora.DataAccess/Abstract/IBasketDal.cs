using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract
{
	public interface IBasketDal : IRepository<Basket>
	{
		void DeleteFromCart(int basketId , int productId);
		void ClearFrommCart(string cartId);
		Basket CartByUserId(string userId);
	}
}
