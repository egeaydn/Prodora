using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore
{
	public class EfCoreCategoryDal : EfCoreGenericRepository<Category, DataContext>, ICategoryDal
	{
		public void DeleteCategory(int categoryId, int productId)
		{
			using (var context = new DataContext())
			{
				var cmd = "DELETE FROM ProductCategories WHERE ProductId=@p0 AND CategoryId=@p1";
				context.Database.ExecuteSqlRaw(cmd, productId, categoryId);
			}
		}

		public Category GetByProducts(int id) 
		{
			using (var context = new DataContext())
			{
				return context.Categories
					.Where(x => x.Id == id)
					.Include(x => x.ProductCategories)
					.ThenInclude(x => x.Product)
					.ThenInclude(x => x.Images)
					.FirstOrDefault();
			}
		}

		public override void Update(Category entity)
		{
			using (var context = new DataContext())
			{
				context.Categories.Remove(entity);
				context.SaveChanges();
			}
		}
	}
}
