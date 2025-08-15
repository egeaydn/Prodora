using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract
{
    /// <summary>
    /// Order entity'si için özel veri erişim işlemlerini tanımlayan interface
    /// Temel CRUD operasyonları IRepository'den kalıtım alır
    /// </summary>
    public interface IOrderDal : IRepository<Order>
    {
        /// <summary>
        /// Belirtilen kullanıcının tüm siparişlerini getirir
        /// </summary>
        /// <param name="userId">Siparişleri getirilecek kullanıcının ID'si</param>
        /// <returns>Kullanıcının sipariş listesi</returns>
        List<Order> GetOrders(string userId);
    }
}
