using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.Business.Abstract
{
	public interface IBasketServices
	{
		void InitialBasket(string userId);
		Basket GetBasketByUserId(string userId);
		void AddToBasket(string userId, int productId, int quantity);
		void DeleteFromBasket(string userId, int productId);
		void ClearBasket(string basketId);
	}
}
