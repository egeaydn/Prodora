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
		void AddToBasket(string userId, int productId, int quantity, int? variantId = null);
		void DeleteFromBasket(string userId, int productId);
		/// <summary>Removes one basket line, whatever the product or variant.</summary>
		void RemoveItem(string userId, int basketItemId);
		void ClearBasket(string basketId);
	}
}
