using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.Business.Abstract
{
	public interface ICommentServices
	{
		Comment GetById(int id);
		void Create(Comment entity);
		void Update(Comment entity);
		void Delete(Comment entity);
		double GetAverageRating(int productId);
		List<Comment>GetCommentByProductId(int productId);
		List<Comment> GetCommentsByUserId(string userId);
		List<Comment> GetCommentsByProductAndUserId(int productId, string userId);
		List<Comment> GetCommentsByProductAndUserName(int productId, string userName);
		List<Comment> GetCommentsByUserName(string userName);
		List<Comment> GetCommentsByProductName(string productName);
		List<Comment> GetCommentsByProductBrand(string brandName);
		List<Comment> GetCommentsByProductPriceRange(decimal minPrice, decimal maxPrice);
		List<Comment> GetCommentsByProductRating(int rating);
		List<Comment> GetCommentsByDateRange(DateTime startDate, DateTime endDate);
		void DeleteFromComment(int productId, string userId);
		void ClearFromComment(string userId);

	}
}
