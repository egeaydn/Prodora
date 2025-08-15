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
        /// <summary>
        /// Belirtilen kullanıcının tüm siparişlerini ürünleri ve resimleri ile birlikte getirir
        /// </summary>
        /// <param name="userId">Siparişleri getirilecek kullanıcının ID'si</param>
        /// <returns>Kullanıcının sipariş listesi, ürünleri ve resimleri ile birlikte</returns>
        public List<Order> GetOrders(string userId)
        {
            using (var context = new DataContext())
            {
                var orders = context.Orders.Include(o => o.OrderItems).ThenInclude(o => o.Product).ThenInclude(o => o.Images).AsQueryable();
                if (!string.IsNullOrEmpty(userId))
                {
                    orders = orders.Where(o => o.UserId == userId);
                }
                return orders.ToList();
            }
        }
    }
}
