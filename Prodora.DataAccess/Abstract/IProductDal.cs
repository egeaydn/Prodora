using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract
{
    /// <summary>
    /// Product entity'si için özel veri erişim işlemlerini tanımlayan interface
    /// Temel CRUD operasyonları IRepository'den kalıtım alır
    /// </summary>
    public interface IProductDal : IRepository<Product>
    {
        /// <summary>
        /// Belirtilen kategoriye ait ürün sayısını döndürür
        /// </summary>
        /// <param name="category">Ürün sayısı alınacak kategori adı</param>
        /// <returns>Kategoriye ait ürün sayısı</returns>
        int GetCountByDCategory(string category);

        /// <summary>
        /// Belirtilen kategoriye ait ürünleri sayfalama ile getirir
        /// </summary>
        /// <param name="category">Ürünlerin getirileceği kategori adı</param>
        /// <param name="page">Sayfa numarası (1'den başlar)</param>
        /// <param name="pageSize">Sayfa başına ürün sayısı</param>
        /// <returns>Kategoriye ait ürünlerin sayfalanmış listesi</returns>
        List<Product> GetProductsByCategory(string category, int page, int pageSize);

        /// <summary>
        /// Ürünü ve kategorilerini günceller
        /// </summary>
        /// <param name="entity">Güncellenecek ürün</param>
        /// <param name="categoryIds">Ürünün ait olacağı kategorilerin ID'leri</param>
        void Update(Product entity, int[] categoryIds);

        /// <summary>
        /// Ürünün detaylarını kategorileri ile birlikte getirir
        /// </summary>
        /// <param name="id">Detayları getirilecek ürünün ID'si</param>
        /// <returns>Kategorileri ile birlikte ürün detayları</returns>
        Product GetProductDetails(int id);
    }
}
