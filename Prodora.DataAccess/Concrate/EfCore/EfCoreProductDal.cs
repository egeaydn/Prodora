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
	public class EfCoreProductDal : EfCoreGenericRepository<Product, DataContext>, IProductDal
	{
		public int GetCountByDCategory(string category)
		{
			using (var context = new DataContext())
			{
				var products = context.Products.AsQueryable();
				if (!string.IsNullOrEmpty(category))
				{
					products = products
						.Include(i => i.ProductCategory)
						.ThenInclude(i => i.Category)
						.Where(i => i.ProductCategory.Any(a => a.Category.Name.ToLower() == category.ToLower()));

					return products.Count();
				}
				else
				{
					return products.Include(i => i.ProductCategory)
								  .ThenInclude(i => i.Category)
								  .Where(i => i.ProductCategory.Any())
								  .Count();
				}
				return 0;
			}
		}

		public Product GetProductDetails(int id)
		{
			using (var context = new DataContext())
			{
				return context.Products
					.Where(i => i.Id == id)
					.Include("Images")
					.Include(i => i.ProductCategory)
					.ThenInclude(i => i.Category)
					.Include(i => i.Comments)
					.FirstOrDefault();
			}
		}

		public List<Product> GetProductsByCategory(string category, int page, int pageSize)
		{
			using (var context = new DataContext())
			{
				var products = context.Products
					.Include("Images")
					.Include(i => i.ProductCategory)
					.ThenInclude(i => i.Category)
					.AsQueryable();

				if (!string.IsNullOrEmpty(category) && category != "all")
				{
					products = products.Where(i => i.ProductCategory.Any(a => a.Category.Name.ToLower() == category.ToLower()));
				}

				return products
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToList();
			}
		}

		public void Update(Product entity, int[] categoryIds)
		{
			using (var context = new DataContext())
			{
				var products = context.Products.Include(i => i.ProductCategory).FirstOrDefault(i => i.Id == entity.Id);

				if (products is not null)
				{
					products.Price = entity.Price;
					products.Name = entity.Name;
					products.Description = entity.Description;
					products.ProductCategory = categoryIds.Select(cartid => new ProductCategory()
					{
						ProductId = entity.Id,
						CategoryId = cartid,
					}).ToList();
					products.Images = entity.Images;
				}
				context.SaveChanges();
			}
		}
		public override void Delete(Product entity)
		{
			using (var context = new DataContext())
			{
				context.Images.RemoveRange(entity.Images);
				context.Products.Remove(entity);
				context.SaveChanges();
			}
		}
		public override List<Product> GetAll(Expression<Func<Product, bool>> filter = null)
		{
			using (var context = new DataContext())
			{
				return filter == null
						? context.Products.Include("Images").ToList()
						: context.Products.Include("Images").Where(filter).ToList();

			}
		}
	}
}

