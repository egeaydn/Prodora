using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract
{
	public interface ICommentDal : IRepository<Comment>
	{
		// Belirli bir ürüne ve kullanıcıya ait yorumu siler.
		void DeleteFromComment(int productId, string userId);

		// Belirli bir kullanıcıya ait tüm yorumları siler.
		void ClearFromComment(string userId);

		// Belirli bir ürünün tüm yorumlarını getirir.
		List<Comment> GetCommentsByProductId(int productId);

		// Belirli bir kullanıcıya ait tüm yorumları getirir.
		List<Comment> GetCommentsByUserId(string userId);

		// Tüm yorumları getirir.
		List<Comment> GetAllComments();

		// Belirli bir ürün ve kullanıcıya ait yorumları getirir.
		List<Comment> GetCommentsByProductAndUserId(int productId, string userId);

		// Belirli bir ürün ve kullanıcı adına ait yorumları getirir.
		List<Comment> GetCommentsByProductAndUserName(int productId, string userName);

		// Kullanıcı adına göre yorumları getirir.
		List<Comment> GetCommentsByUserName(string userName);

		// Ürün adına göre yapılan yorumları getirir.
		List<Comment> GetCommentsByProductName(string productName);

		// Belirli bir kategoriye ait ürünlerin yorumlarını getirir.

		// Belirli bir markaya ait ürünlerin yorumlarını getirir.
		List<Comment> GetCommentsByProductBrand(string brandName);

		// Belirtilen fiyat aralığındaki ürünlere ait yorumları getirir.
		List<Comment> GetCommentsByProductPriceRange(decimal minPrice, decimal maxPrice);

		// Belirli bir puan (rating) değerine sahip ürünlerin yorumlarını getirir.
		List<Comment> GetCommentsByProductRating(int rating);

		// Belirtilen tarih aralığında yapılan yorumları getirir.
		List<Comment> GetCommentsByDateRange(DateTime startDate, DateTime endDate);
		List<Comment> GetCommetsByProductId(int productId);
		double GetAverageRating(int productId);
	}
}
