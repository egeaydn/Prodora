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
            modelBuilder.Entity<OrderItem>().HasOne(i => i.Product).WithMany()
                .HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderItem>().Property(i => i.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Product>().Property(i => i.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.RequestId).HasMaxLength(64);
            modelBuilder.Entity<Order>().HasIndex(o => o.RequestId).IsUnique().HasFilter("[RequestId] IS NOT NULL");
            modelBuilder.Entity<Order>().Property(o => o.DiscountAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.CouponCode).HasMaxLength(32);
            modelBuilder.Entity<Coupon>().Property(c => c.Code).HasMaxLength(32).IsRequired();
            modelBuilder.Entity<Coupon>().HasIndex(c => c.Code).IsUnique();
            modelBuilder.Entity<Coupon>().Property(c => c.Value).HasPrecision(18, 2);
            modelBuilder.Entity<Coupon>().Property(c => c.MinSubtotal).HasPrecision(18, 2);
            modelBuilder.Entity<CouponRedemption>().Property(r => r.UserId).HasMaxLength(450).IsRequired();
            modelBuilder.Entity<CouponRedemption>().HasIndex(r => new { r.CouponId, r.UserId }).IsUnique();
            modelBuilder.Entity<CouponRedemption>().HasOne(r => r.Coupon).WithMany().HasForeignKey(r => r.CouponId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CouponRedemption>().HasOne(r => r.Order).WithMany().HasForeignKey(r => r.OrderId).OnDelete(DeleteBehavior.Cascade);
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

        public DbSet<Coupon> Coupons { get; set; }

        public DbSet<CouponRedemption> CouponRedemptions { get; set; }
    }
}
