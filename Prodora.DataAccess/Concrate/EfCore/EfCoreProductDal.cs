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
    /// <summary>
    /// Product entity'si için Entity Framework Core implementasyonu
    /// IProductDal interface'ini implement eder ve özel ürün işlemlerini sağlar
    /// </summary>
    public class EfCoreProductDal : EfCoreGenericRepository<Product, DataContext>, IProductDal
    {
        /// <summary>
        /// Belirtilen kategoriye ait ürün sayısını döndürür
        /// </summary>
        /// <param name="category">Ürün sayısı alınacak kategori adı</param>
        /// <returns>Kategoriye ait ürün sayısı</returns>
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

        /// <summary>
        /// Ürünün detaylarını kategorileri, resimleri ve yorumları ile birlikte getirir
        /// </summary>
        /// <param name="id">Detayları getirilecek ürünün ID'si</param>
        /// <returns>Kategorileri, resimleri ve yorumları ile birlikte ürün detayları</returns>
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

        /// <summary>
        /// Belirtilen kategoriye ait ürünleri sayfalama ile getirir
        /// </summary>
        /// <param name="category">Ürünlerin getirileceği kategori adı</param>
        /// <param name="page">Sayfa numarası (1'den başlar)</param>
        /// <param name="pageSize">Sayfa başına ürün sayısı</param>
        /// <returns>Kategoriye ait ürünlerin sayfalanmış listesi</returns>
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

        /// <summary>
        /// Ürünü ve kategorilerini günceller
        /// </summary>
        /// <param name="entity">Güncellenecek ürün</param>
        /// <param name="categoryIds">Ürünün ait olacağı kategorilerin ID'leri</param>
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

        /// <summary>
        /// Ürünü ve ilişkili resimlerini veritabanından siler
        /// </summary>
        /// <param name="entity">Silinecek ürün</param>
        public override void Delete(Product entity)
        {
            using (var context = new DataContext())
            {
                context.Images.RemoveRange(entity.Images);
                context.Products.Remove(entity);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Tüm ürünleri veya belirtilen filtreye uyan ürünleri resimleri ile birlikte getirir
        /// </summary>
        /// <param name="filter">Ürünleri filtrelemek için lambda expression</param>
        /// <returns>Filtreye uyan ürünlerin resimleri ile birlikte listesi</returns>
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

