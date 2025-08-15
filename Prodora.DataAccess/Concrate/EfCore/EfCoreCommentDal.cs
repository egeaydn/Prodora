using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore
{
    /// <summary>
    /// Comment entity'si için Entity Framework Core implementasyonu
    /// ICommentDal interface'ini implement eder ve özel yorum işlemlerini sağlar
    /// </summary>
    public class EfCoreCommentDal : EfCoreGenericRepository<Comment, DataContext>, ICommentDal
    {
        /// <summary>
        /// Belirli bir kullanıcıya ait tüm yorumları siler
        /// </summary>
        /// <param name="userId">Tüm yorumları silinecek kullanıcının ID'si</param>
        public void ClearFromComment(string userId)
        {
            using (var context = new DataContext())
            {
                var comments = context.Comments.Where(c => c.UserId == userId).ToList();
                context.Comments.RemoveRange(comments);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Belirli bir ürüne ve kullanıcıya ait yorumu siler
        /// </summary>
        /// <param name="productId">Yorumun silineceği ürünün ID'si</param>
        /// <param name="userId">Yorumu silinecek kullanıcının ID'si</param>
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

        /// <summary>
        /// Tüm yorumları getirir
        /// </summary>
        /// <returns>Tüm yorumların listesi</returns>
        public List<Comment> GetAllComments()
        {
            using (var context = new DataContext())
            {
                return context.Comments.ToList();
            }
        }

        /// <summary>
        /// Belirli bir ürünün ortalama puanını hesaplar
        /// </summary>
        /// <param name="productId">Ortalama puanı hesaplanacak ürünün ID'si</param>
        /// <returns>Ürünün ortalama puanı, yorum yoksa 0</returns>
        public double GetAverageRating(int productId)
        {
            using (var context = new DataContext())
            {
                var reviews = context.Comments.Where(c => c.ProductId == productId);
                return reviews.Any() ? reviews.Average(c => c.Raitings) : 0; // Return 0 if there are no reviews for the product
            }
        }

        /// <summary>
        /// Belirtilen tarih aralığında yapılan yorumları getirir
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Belirtilen tarih aralığında yapılan yorumların listesi</returns>
        public List<Comment> GetCommentsByDateRange(DateTime startDate, DateTime endDate)
        {
            using (var context = new DataContext())
            {
                return context.Comments
                    .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                    .ToList();
            }
        }

        /// <summary>
        /// Belirli bir ürün ve kullanıcıya ait yorumları getirir
        /// </summary>
        /// <param name="productId">Ürün ID'si</param>
        /// <param name="userId">Kullanıcı ID'si</param>
        /// <returns>Belirtilen ürün ve kullanıcıya ait yorumların listesi</returns>
        public List<Comment> GetCommentsByProductAndUserId(int productId, string userId)
        {
            using (var context = new DataContext())
            {
                return context.Comments
                    .Where(c => c.ProductId == productId && c.UserId == userId)
                    .ToList();
            }
        }

        /// <summary>
        /// Belirli bir ürün ve kullanıcı adına ait yorumları getirir
        /// </summary>
        /// <param name="productId">Ürün ID'si</param>
        /// <param name="userName">Kullanıcı adı</param>
        /// <returns>Belirtilen ürün ve kullanıcı adına ait yorumların listesi</returns>
        public List<Comment> GetCommentsByProductAndUserName(int productId, string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Belirli bir markaya ait ürünlerin yorumlarını getirir
        /// </summary>
        /// <param name="brandName">Marka adı</param>
        /// <returns>Belirtilen markaya ait ürünlerin yorumlarının listesi</returns>
        public List<Comment> GetCommentsByProductBrand(string brandName)
        {
            using (var context = new DataContext())
            {
                return context.Comments
                    .Where(c => c.Product.Brand == brandName)
                    .ToList();
            }
        }

        /// <summary>
        /// Belirli bir ürünün tüm yorumlarını getirir
        /// </summary>
        /// <param name="productId">Yorumları getirilecek ürünün ID'si</param>
        /// <returns>Ürüne ait yorumların listesi</returns>
        public List<Comment> GetCommentsByProductId(int productId)
        {
            using (var context = new DataContext())
            {
                return context.Comments.Where(c => c.ProductId == productId).ToList();
            }
        }

        /// <summary>
        /// Ürün adına göre yapılan yorumları getirir
        /// </summary>
        /// <param name="productName">Ürün adı</param>
        /// <returns>Belirtilen ürün adına ait yorumların listesi</returns>
        public List<Comment> GetCommentsByProductName(string productName)
        {
            using (var context = new DataContext())
            {
                return context.Comments
                    .Where(c => c.Product.Name.Contains(productName))
                    .ToList();
            }
        }

        /// <summary>
        /// Belirtilen fiyat aralığındaki ürünlere ait yorumları getirir
        /// </summary>
        /// <param name="minPrice">Minimum fiyat</param>
        /// <param name="maxPrice">Maksimum fiyat</param>
        /// <returns>Belirtilen fiyat aralığındaki ürünlerin yorumlarının listesi</returns>
        public List<Comment> GetCommentsByProductPriceRange(decimal minPrice, decimal maxPrice)
        {
            using (var context = new DataContext())
            {
                return context.Comments
                    .Where(c => c.Product.Price >= minPrice && c.Product.Price <= maxPrice)
                    .ToList();
            }
        }

        /// <summary>
        /// Belirli bir puan (rating) değerine sahip ürünlerin yorumlarını getirir
        /// </summary>
        /// <param name="rating">Aranacak puan değeri</param>
        /// <returns>Belirtilen puana sahip ürünlerin yorumlarının listesi</returns>
        public List<Comment> GetCommentsByProductRating(int rating)
        {
            using (var context = new DataContext())
            {
                return context.Comments
                    .Where(c => c.Raitings == rating)
                    .ToList();
            }
        }

        /// <summary>
        /// Belirli bir kullanıcıya ait tüm yorumları getirir
        /// </summary>
        /// <param name="userId">Yorumları getirilecek kullanıcının ID'si</param>
        /// <returns>Kullanıcıya ait yorumların listesi</returns>
        public List<Comment> GetCommentsByUserId(string userId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Kullanıcı adına göre yorumları getirir
        /// </summary>
        /// <param name="userName">Kullanıcı adı</param>
        /// <returns>Belirtilen kullanıcı adına ait yorumların listesi</returns>
        public List<Comment> GetCommentsByUserName(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Belirli bir ürünün yorumlarını getirir (alternatif metod)
        /// </summary>
        /// <param name="productId">Yorumları getirilecek ürünün ID'si</param>
        /// <returns>Ürüne ait yorumların listesi</returns>
        public List<Comment> GetCommetsByProductId(int productId)
        {
            throw new NotImplementedException();
        }
    }
}
