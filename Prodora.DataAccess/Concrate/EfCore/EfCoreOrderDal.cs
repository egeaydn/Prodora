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
    /// Order entity'si için Entity Framework Core implementasyonu
    /// IOrderDal interface'ini implement eder ve özel sipariş işlemlerini sağlar
    /// </summary>
    public class EfCoreOrderDal : EfCoreGenericRepository<Order, DataContext>, IOrderDal
    {
        public EfCoreOrderDal(IDbContextFactory<DataContext> contextFactory) : base(contextFactory) { }

        /// <summary>
        /// Belirtilen kullanıcının tüm siparişlerini ürünleri ve resimleri ile birlikte getirir
        /// </summary>
        /// <param name="userId">Siparişleri getirilecek kullanıcının ID'si</param>
        /// <returns>Kullanıcının sipariş listesi, ürünleri ve resimleri ile birlikte</returns>
        public List<Order> GetOrders(string userId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                if (string.IsNullOrWhiteSpace(userId)) return new List<Order>();
                var orders = context.Orders.Include(o => o.OrderItems).ThenInclude(o => o.Product).ThenInclude(o => o.Images).AsQueryable();
                if (!string.IsNullOrEmpty(userId))
                {
                    orders = orders.Where(o => o.UserId == userId);
                }
                return orders.OrderByDescending(o => o.OrderDate).ToList();
            }
        }

        public Order? GetOrder(int orderId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Orders.AsNoTracking()
                .Include(o => o.OrderItems).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
                .FirstOrDefault(o => o.Id == orderId);
        }

        /// <param name="bucket">null/"all": tümü, "review": ödeme kontrolü bekleyenler, "open": ilerletilebilir siparişler, "closed": sonuçlanmış siparişler.</param>
        public List<Order> GetStoreOrders(string? bucket = null)
        {
            using var context = _contextFactory.CreateDbContext();
            return FilterStoreOrders(context.Orders.AsNoTracking(), bucket).Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id).ToList();
        }

        public StoreOrderPage GetStoreOrdersPage(string? bucket, int page, int pageSize)
        {
            using var context = _contextFactory.CreateDbContext();
            pageSize = Math.Clamp(pageSize, 1, 100);
            var query = FilterStoreOrders(context.Orders.AsNoTracking(), bucket);
            var total = query.Count();
            var pages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            page = Math.Clamp(page, 1, pages);
            var orders = query.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id)
                .Skip((page - 1) * pageSize).Take(pageSize).Include(o => o.OrderItems).ToList();
            return new(orders, total, page, pages);
        }

        private static IQueryable<Order> FilterStoreOrders(IQueryable<Order> orders, string? bucket)
        {
            // Resolve the business rules to a status list, then filter in SQL before paging.
            var statuses = Enum.GetValues<OrderStatus>();
            var selected = bucket?.Trim().ToLowerInvariant() switch
            {
                "review" => statuses.Where(OrderFulfillment.NeedsPaymentReview).ToArray(),
                "open" => statuses.Where(s => OrderFulfillment.NextStatuses(s).Count > 0).ToArray(),
                "closed" => statuses.Where(s => OrderFulfillment.NextStatuses(s).Count == 0 && !OrderFulfillment.NeedsPaymentReview(s)).ToArray(),
                _ => null
            };
            return selected == null ? orders : orders.Where(o => selected.Contains(o.OrderEnums));
        }

        public List<Order> GetOrdersBetween(DateTime from, DateTime toExclusive)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Orders.AsNoTracking().Include(o => o.OrderItems)
                .Where(o => o.OrderDate >= from && o.OrderDate < toExclusive)
                .OrderBy(o => o.OrderDate).ThenBy(o => o.Id).ToList();
        }

        public (Order Order, OrderStatus Previous) SetStatus(int orderId, OrderStatus next, string? trackingNumber = null) =>
            DeadlockRetry.Run(() => SetStatusOnce(orderId, next, trackingNumber));

        private (Order Order, OrderStatus Previous) SetStatusOnce(int orderId, OrderStatus next, string? trackingNumber)
        {
            using var context = _contextFactory.CreateDbContext();
            using var transaction = context.Database.BeginTransaction();
            var order = context.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == orderId)
                ?? throw new StoreValidationException("Sipariş bulunamadı.");
            var previous = order.OrderEnums;
            if (!OrderFulfillment.CanTransition(previous, next))
                throw new StoreValidationException("Bu sipariş durumu seçilen duruma geçirilemez.");
            if (next == OrderStatus.Shipped)
                order.TrackingNumber = OrderFulfillment.NormalizeTrackingNumber(trackingNumber)
                    ?? throw new StoreValidationException("Kargo takip numarasını gir (harf, rakam ve tire, en fazla 40 karakter).");
            order.OrderEnums = next;
            if (next == OrderStatus.Cancelled)
            {
                StockLedger.Release(context, order);
                CouponLedger.Release(context, order.Id);
            }
            context.SaveChanges();
            transaction.Commit();
            return (order, previous);
        }
    }
}
