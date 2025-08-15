using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract
{
    /// <summary>
    /// Category entity'si için özel veri erişim işlemlerini tanımlayan interface
    /// Temel CRUD operasyonları IRepository'den kalıtım alır
    /// </summary>
    public interface ICategoryDal : IRepository<Category>
    {
        /// <summary>
        /// Belirtilen üründen kategoriyi kaldırır (ilişkiyi siler)
        /// </summary>
        /// <param name="categoryId">Kaldırılacak kategorinin ID'si</param>
        /// <param name="productId">Kategorinin kaldırılacağı ürünün ID'si</param>
        void DeleteCategory(int categoryId, int productId);

        /// <summary>
        /// Belirtilen ID'ye sahip kategoriyi ürünleri ile birlikte getirir
        /// </summary>
        /// <param name="id">Ürünleri ile birlikte getirilecek kategorinin ID'si</param>
        /// <returns>Ürünleri ile birlikte kategori</returns>
        Category GetByProducts(int id);
    }
}
