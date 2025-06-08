using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Business.Abstract;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.Business.Concrate
{
	public class CommentManager : ICommentServices
	{
		private ICommentDal _commentDal;
		public CommentManager(ICommentDal commentDal)
		{
			_commentDal = commentDal;
		}
		public void Create(Comment entity)
		{
			_commentDal.Create(entity);
		}

		public void Delete(Comment entity)
		{
			_commentDal.Delete(entity);
		}

		public double GetAverageRating(int productId)
		{
			return _commentDal.GetAverageRating(productId);
		}

		public Comment GetById(int id)
		{
			return _commentDal.GetById(id);
		}

		public List<Comment> GetCommentByProductId(int productId)
		{
			return _commentDal.GetCommentsByProductId(productId);
		}

		public void Update(Comment entity)
		{
			_commentDal.Update(entity);
		}

		public List<Comment> GetCommentsByUserId(string userId)
		{
			return _commentDal.GetCommentsByUserId(userId);
		}

		public List<Comment> GetCommentsByProductAndUserId(int productId, string userId)
		{
			return _commentDal.GetCommentsByProductAndUserId(productId, userId);
		}

		public List<Comment> GetCommentsByProductAndUserName(int productId, string userName)
		{
			return _commentDal.GetCommentsByProductAndUserName(productId, userName);
		}

		public List<Comment> GetCommentsByUserName(string userName)
		{
			return _commentDal.GetCommentsByUserName(userName);
		}

		public List<Comment> GetCommentsByProductName(string productName)
		{
			return _commentDal.GetCommentsByProductName(productName);
		}

		public List<Comment> GetCommentsByProductBrand(string brandName)
		{
			return _commentDal.GetCommentsByProductBrand(brandName);
		}

		public List<Comment> GetCommentsByProductPriceRange(decimal minPrice, decimal maxPrice)
		{
			return _commentDal.GetCommentsByProductPriceRange(minPrice, maxPrice);
		}

		public List<Comment> GetCommentsByProductRating(int rating)
		{
			return _commentDal.GetCommentsByProductRating(rating);
		}

		public List<Comment> GetCommentsByDateRange(DateTime startDate, DateTime endDate)
		{
			return _commentDal.GetCommentsByDateRange(startDate, endDate);
		}

		public void DeleteFromComment(int productId, string userId)
		{
			_commentDal.DeleteFromComment(productId, userId);
		}

		public void ClearFromComment(string userId)
		{
			_commentDal.ClearFromComment(userId);
		}
	}
}
