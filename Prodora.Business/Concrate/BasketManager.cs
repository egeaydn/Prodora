using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Prodora.Business.Abstract;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.Business.Concrate
{
	public class BasketManager : IBasketServices
	{
		private IBasketDal _basketDal;
		public BasketManager(IBasketDal basketDal)
		{
			_basketDal = basketDal;
		}

		public void AddToBasket(string userId, int productId, int quantity)
		{
			var cart = GetBasketByUserId(userId);

			if (cart is not null)
			{
				var index = cart.BasketItems.FindIndex(x => x.ProductId == productId);

				if (index < 0)
				{
					cart.BasketItems.Add(
						new BasketItem()
						{
							ProductId = productId,
							Quantity = quantity,
							BasketId = cart.Id
						}
					);
				}
				else
				{
					cart.BasketItems[index].Quantity += quantity;
				}
			}

			_basketDal.Update(cart); // Dataaccess aracılığıyla sepeti günceller.
		}
		

		public void ClearBasket(string basketId)
		{
			_basketDal.ClearFrommCart(basketId);
		}

		public void DeleteFromBasket(string userId, int productId)
		{
			var cart = GetBasketByUserId(userId);

			if (cart != null)
			{
				_basketDal.DeleteFromCart(cart.Id, productId);
			}
		}

		public Basket GetBasketByUserId(string userId)
		{
			return _basketDal.CartByUserId(userId);
		}

		public void InitialBasket(string userId)
		{
			Basket basket = new Basket()
			{
				UserId = userId,
			};
			_basketDal.Create(basket);
		}
	}
}
