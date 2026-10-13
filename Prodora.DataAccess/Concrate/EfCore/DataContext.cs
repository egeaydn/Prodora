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
            modelBuilder.Entity<WishlistItem>().Property(w => w.UserId).HasMaxLength(450).IsRequired();
            modelBuilder.Entity<WishlistItem>().HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
            modelBuilder.Entity<WishlistItem>().HasOne(w => w.Product).WithMany().HasForeignKey(w => w.ProductId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SavedAddress>().Property(a => a.UserId).HasMaxLength(450).IsRequired();
            modelBuilder.Entity<SavedAddress>().Property(a => a.Title).HasMaxLength(30);
            modelBuilder.Entity<SavedAddress>().Property(a => a.FirstName).HasMaxLength(50);
            modelBuilder.Entity<SavedAddress>().Property(a => a.LastName).HasMaxLength(50);
            modelBuilder.Entity<SavedAddress>().Property(a => a.Phone).HasMaxLength(20);
            modelBuilder.Entity<SavedAddress>().Property(a => a.Address).HasMaxLength(250);
            modelBuilder.Entity<SavedAddress>().Property(a => a.City).HasMaxLength(50);
            modelBuilder.Entity<SavedAddress>().HasIndex(a => a.UserId);
            // At most one default address per customer, enforced by the database.
            modelBuilder.Entity<SavedAddress>().HasIndex(a => a.UserId).IsUnique().HasFilter("[IsDefault] = 1").HasDatabaseName("IX_SavedAddresses_UserId_Default");
            modelBuilder.Entity<AuditEntry>().Property(e => e.ActorId).HasMaxLength(450);
            modelBuilder.Entity<AuditEntry>().Property(e => e.ActorName).HasMaxLength(AuditEntry.ActorNameLength);
            modelBuilder.Entity<AuditEntry>().Property(e => e.Action).HasMaxLength(AuditEntry.ActionLength).IsRequired();
            modelBuilder.Entity<AuditEntry>().Property(e => e.Target).HasMaxLength(AuditEntry.TargetLength);
            modelBuilder.Entity<AuditEntry>().Property(e => e.Detail).HasMaxLength(AuditEntry.DetailLength);
            modelBuilder.Entity<AuditEntry>().Property(e => e.Ip).HasMaxLength(AuditEntry.IpLength);
            modelBuilder.Entity<AuditEntry>().HasIndex(e => e.At);
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

        public DbSet<WishlistItem> WishlistItems { get; set; }

        public DbSet<SavedAddress> SavedAddresses { get; set; }

        public DbSet<AuditEntry> AuditEntries { get; set; }
    }
}
