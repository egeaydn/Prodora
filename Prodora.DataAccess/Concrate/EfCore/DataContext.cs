using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore
{
    /// <summary>
    /// Entity Framework Core veritabanı bağlam sınıfı
    /// Tüm entity'lerin DbSet'lerini ve konfigürasyonlarını içerir
    /// </summary>
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        /// <summary>
        /// Entity modellerinin konfigürasyonunu yapar
        /// Composite key'ler ve ilişkiler burada tanımlanır
        /// </summary>
        /// <param name="modelBuilder">Model yapılandırıcı</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ProductCategory entity'si için composite key tanımlama
            modelBuilder.Entity<ProductCategory>().HasKey(p => new { p.ProductId, p.CategoryId });
        }

        /// <summary>
        /// Ürünler için DbSet
        /// </summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>
        /// Kategoriler için DbSet
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Resimler için DbSet
        /// </summary>
        public DbSet<Image> Images { get; set; }

        /// <summary>
        /// Yorumlar için DbSet
        /// </summary>
        public DbSet<Comment> Comments { get; set; }

        /// <summary>
        /// Sepetler için DbSet
        /// </summary>
        public DbSet<Basket> Baskets { get; set; }

        /// <summary>
        /// Siparişler için DbSet
        /// </summary>
        public DbSet<Order> Orders { get; set; }
    }
}
