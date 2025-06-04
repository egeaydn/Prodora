using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore
{
	public class EfCoreCommentDal : EfCoreGenericRepository<Comment, DataContext>, ICommentDal
	{
		public void ClearFromComment(string userId)
		{
			using (var context = new DataContext())
			{
				var comments = context.Comments.Where(c => c.UserId == userId).ToList();
				context.Comments.RemoveRange(comments);
				context.SaveChanges();
			}
		}


		public void DeleteFromComment(int productId, string userId)
		{
			using (var context = new DataContext())
			{
				var comment = context.Comments.FirstOrDefault(c => c.ProductId == productId && c.UserId == userId);
				if (comment != null)
				{
					context.Comments.Remove(comment);
					context.SaveChanges();
				}
			}
		}


		public List<Comment> GetAllComments()
		{
			using (var context = new DataContext())
			{
				return context.Comments.ToList();
			}
		}


		public double GetAverageRating(int productId)
		{
			using (var context = new DataContext()) {
				// Assuming DataContext is properly configured to connect to your database
				var rewiews = context.Comments.Where(c => c.ProductId == productId);
				return rewiews.Any() ? rewiews.Average(c => c.Raitings): 0; // Return 0 if there are no reviews for the product

			}
			;
		}

		public List<Comment> GetCommentsByDateRange(DateTime startDate, DateTime endDate)
		{
			using (var context = new DataContext())
			{
				return context.Comments
					.Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
					.ToList();
			}
		}


		public List<Comment> GetCommentsByProductAndUserId(int productId, string userId)
		{
			using (var context = new DataContext())
			{
				return context.Comments
					.Where(c => c.ProductId == productId && c.UserId == userId)
					.ToList();
			}
		}


		public List<Comment> GetCommentsByProductAndUserName(int productId, string userName)
		{
			throw new NotImplementedException();
		}

		public List<Comment> GetCommentsByProductBrand(string brandName)
		{
			using (var context = new DataContext())
			{
				return context.Comments
					.Where(c => c.Product.Brand == brandName)
					.ToList();
			}
		}

		public List<Comment> GetCommentsByProductId(int productId)
		{
			using (var context = new DataContext())
			{
				// Assuming DataContext is properly configured to connect to your database
				return context.Comments.Where(c => c.ProductId == productId).ToList();
			}
		}

		public List<Comment> GetCommentsByProductName(string productName)
		{
			using (var context = new DataContext())
			{
				return context.Comments
					.Where(c => c.Product.Name.Contains(productName))
					.ToList();
			}
		}


		public List<Comment> GetCommentsByProductPriceRange(decimal minPrice, decimal maxPrice)
		{
			using (var context = new DataContext())
			{
				return context.Comments
					.Where(c => c.Product.Price >= minPrice && c.Product.Price <= maxPrice)
					.ToList();
			}
		}


		public List<Comment> GetCommentsByProductRating(int rating)
		{
			using (var context = new DataContext())
			{
				return context.Comments
					.Where(c => c.Raitings == rating)
					.ToList();
			}
		}


		public List<Comment> GetCommentsByUserId(string userId)
		{
			throw new NotImplementedException();
		}

		public List<Comment> GetCommentsByUserName(string userName)
		{
			throw new NotImplementedException();
		}

		public List<Comment> GetCommetsByProductId(int productId)
		{
			throw new NotImplementedException();
		}
	}
}
