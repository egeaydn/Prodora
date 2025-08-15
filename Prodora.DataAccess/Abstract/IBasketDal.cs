using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract
{
    /// <summary>
    /// Basket entity'si için özel veri erişim işlemlerini tanımlayan interface
    /// Temel CRUD operasyonları IRepository'den kalıtım alır
    /// </summary>
    public interface IBasketDal : IRepository<Basket>
    {
        /// <summary>
        /// Sepetten belirtilen ürünü kaldırır
        /// </summary>
        /// <param name="basketId">Sepet ID'si</param>
        /// <param name="productId">Sepetten kaldırılacak ürünün ID'si</param>
        void DeleteFromCart(int basketId, int productId);

        /// <summary>
        /// Belirtilen sepet ID'sine sahip sepeti tamamen temizler
        /// </summary>
        /// <param name="cartId">Temizlenecek sepetin ID'si</param>
        void ClearFrommCart(string cartId);

        /// <summary>
        /// Belirtilen kullanıcının sepetini getirir
        /// </summary>
        /// <param name="userId">Sepeti getirilecek kullanıcının ID'si</param>
        /// <returns>Kullanıcının sepeti</returns>
        Basket CartByUserId(string userId);
    }
}
