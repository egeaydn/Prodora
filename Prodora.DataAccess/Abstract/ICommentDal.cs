using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract
{
    /// <summary>
    /// Comment entity'si için özel veri erişim işlemlerini tanımlayan interface
    /// Temel CRUD operasyonları IRepository'den kalıtım alır
    /// </summary>
    public interface ICommentDal : IRepository<Comment>
    {
        /// <summary>
        /// Belirli bir ürüne ve kullanıcıya ait yorumu siler
        /// </summary>
        /// <param name="productId">Yorumun silineceği ürünün ID'si</param>
        /// <param name="userId">Yorumu silinecek kullanıcının ID'si</param>
        void DeleteFromComment(int productId, string userId);

        /// <summary>
        /// Belirli bir kullanıcıya ait tüm yorumları siler
        /// </summary>
        /// <param name="userId">Tüm yorumları silinecek kullanıcının ID'si</param>
        void ClearFromComment(string userId);

        /// <summary>
        /// Belirli bir ürünün tüm yorumlarını getirir
        /// </summary>
        /// <param name="productId">Yorumları getirilecek ürünün ID'si</param>
        /// <returns>Ürüne ait yorumların listesi</returns>
        List<Comment> GetCommentsByProductId(int productId);

        /// <summary>
        /// Belirli bir kullanıcıya ait tüm yorumları getirir
        /// </summary>
        /// <param name="userId">Yorumları getirilecek kullanıcının ID'si</param>
        /// <returns>Kullanıcıya ait yorumların listesi</returns>
        List<Comment> GetCommentsByUserId(string userId);

        /// <summary>
        /// Tüm yorumları getirir
        /// </summary>
        /// <returns>Tüm yorumların listesi</returns>
        List<Comment> GetAllComments();

        /// <summary>
        /// Belirli bir ürün ve kullanıcıya ait yorumları getirir
        /// </summary>
        /// <param name="productId">Ürün ID'si</param>
        /// <param name="userId">Kullanıcı ID'si</param>
        /// <returns>Belirtilen ürün ve kullanıcıya ait yorumların listesi</returns>
        List<Comment> GetCommentsByProductAndUserId(int productId, string userId);

        /// <summary>
        /// Belirli bir ürün ve kullanıcı adına ait yorumları getirir
        /// </summary>
        /// <param name="productId">Ürün ID'si</param>
        /// <param name="userName">Kullanıcı adı</param>
        /// <returns>Belirtilen ürün ve kullanıcı adına ait yorumların listesi</returns>
        List<Comment> GetCommentsByProductAndUserName(int productId, string userName);

        /// <summary>
        /// Kullanıcı adına göre yorumları getirir
        /// </summary>
        /// <param name="userName">Kullanıcı adı</param>
        /// <returns>Belirtilen kullanıcı adına ait yorumların listesi</returns>
        List<Comment> GetCommentsByUserName(string userName);

        /// <summary>
        /// Ürün adına göre yapılan yorumları getirir
        /// </summary>
        /// <param name="productName">Ürün adı</param>
        /// <returns>Belirtilen ürün adına ait yorumların listesi</returns>
        List<Comment> GetCommentsByProductName(string productName);

        /// <summary>
        /// Belirli bir markaya ait ürünlerin yorumlarını getirir
        /// </summary>
        /// <param name="brandName">Marka adı</param>
        /// <returns>Belirtilen markaya ait ürünlerin yorumlarının listesi</returns>
        List<Comment> GetCommentsByProductBrand(string brandName);

        /// <summary>
        /// Belirtilen fiyat aralığındaki ürünlere ait yorumları getirir
        /// </summary>
        /// <param name="minPrice">Minimum fiyat</param>
        /// <param name="maxPrice">Maksimum fiyat</param>
        /// <returns>Belirtilen fiyat aralığındaki ürünlerin yorumlarının listesi</returns>
        List<Comment> GetCommentsByProductPriceRange(decimal minPrice, decimal maxPrice);

        /// <summary>
        /// Belirli bir puan (rating) değerine sahip ürünlerin yorumlarını getirir
        /// </summary>
        /// <param name="rating">Aranacak puan değeri</param>
        /// <returns>Belirtilen puana sahip ürünlerin yorumlarının listesi</returns>
        List<Comment> GetCommentsByProductRating(int rating);

        /// <summary>
        /// Belirtilen tarih aralığında yapılan yorumları getirir
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Belirtilen tarih aralığında yapılan yorumların listesi</returns>
        List<Comment> GetCommentsByDateRange(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Belirli bir ürünün yorumlarını getirir (alternatif metod)
        /// </summary>
        /// <param name="productId">Yorumları getirilecek ürünün ID'si</param>
        /// <returns>Ürüne ait yorumların listesi</returns>
        List<Comment> GetCommetsByProductId(int productId);

        /// <summary>
        /// Belirli bir ürünün ortalama puanını hesaplar
        /// </summary>
        /// <param name="productId">Ortalama puanı hesaplanacak ürünün ID'si</param>
        /// <returns>Ürünün ortalama puanı</returns>
        double GetAverageRating(int productId);

        /// <summary>
        /// Sadece aktif kullanıcıları olan yorumları getirir (silinmemiş kullanıcılar)
        /// </summary>
        /// <param name="productId">Yorumları getirilecek ürünün ID'si</param>
        /// <returns>Aktif kullanıcılarına ait yorumların listesi</returns>
        List<Comment> GetActiveUserCommentsByProductId(int productId);
    }
}
