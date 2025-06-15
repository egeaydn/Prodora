using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore
{
	public class EfCoreBasketDal : EfCoreGenericRepository<Basket, DataContext>, IBasketDal
	{
		public Basket CartByUserId(string userId)
		{
			using (var context = new DataContext())
			{
				return context.Baskets
					.Include(i => i.BasketItems)
					.ThenInclude(i => i.Product)
					.ThenInclude(i => i.Images)
					.FirstOrDefault(i => i.UserId == userId);
			}
		}

		public void ClearFrommCart(string cartId)
		{
			using (var context = new DataContext())
			{
				var cmd = @"delete from BasketItem where BasketId=@p0";
				context.Database.ExecuteSqlRaw(cmd, cartId);
			}
		}

		public void DeleteFromCart(int basketId, int productId)
		{
			using(var context = new DataContext())
			{
				var cmd = @"delete from BasketItem where BasketId=@p0 and ProductId=@p1";
				context.Database.ExecuteSqlRaw(cmd, basketId, productId);

			}
		}

		public override void Update(Basket entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity), "Güncellenmek istenen basket boş!");

			using (var context = new DataContext())
			{
				context.Baskets.Update(entity);
				context.SaveChanges();
			}
		}

	}
}
