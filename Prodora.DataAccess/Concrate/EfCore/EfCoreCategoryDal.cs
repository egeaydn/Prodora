using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore
{
    /// <summary>
    /// Category entity'si için Entity Framework Core implementasyonu
    /// ICategoryDal interface'ini implement eder ve özel kategori işlemlerini sağlar
    /// </summary>
    public class EfCoreCategoryDal : EfCoreGenericRepository<Category, DataContext>, ICategoryDal
    {
        /// <summary>
        /// Belirtilen üründen kategoriyi kaldırır (ilişkiyi siler)
        /// Raw SQL kullanarak ProductCategories tablosundan kayıt siler
        /// </summary>
        /// <param name="categoryId">Kaldırılacak kategorinin ID'si</param>
        /// <param name="productId">Kategorinin kaldırılacağı ürünün ID'si</param>
        public void DeleteCategory(int categoryId, int productId)
        {
            using (var context = new DataContext())
            {
                var cmd = "DELETE FROM ProductCategories WHERE ProductId=@p0 AND CategoryId=@p1";
                context.Database.ExecuteSqlRaw(cmd, productId, categoryId);
            }
        }

        /// <summary>
        /// Belirtilen ID'ye sahip kategoriyi ürünleri ve resimleri ile birlikte getirir
        /// </summary>
        /// <param name="id">Ürünleri ile birlikte getirilecek kategorinin ID'si</param>
        /// <returns>Ürünleri ve resimleri ile birlikte kategori</returns>
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

        /// <summary>
        /// Kategoriyi veritabanından siler (Update metodunu override ederek Delete işlemi yapar)
        /// </summary>
        /// <param name="entity">Silinecek kategori</param>
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
