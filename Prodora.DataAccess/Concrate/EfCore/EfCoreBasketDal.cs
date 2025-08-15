using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore
{
    /// <summary>
    /// Basket entity'si için Entity Framework Core implementasyonu
    /// IBasketDal interface'ini implement eder ve özel sepet işlemlerini sağlar
    /// </summary>
    public class EfCoreBasketDal : EfCoreGenericRepository<Basket, DataContext>, IBasketDal
    {
        /// <summary>
        /// Belirtilen kullanıcının sepetini ürünleri ve resimleri ile birlikte getirir
        /// </summary>
        /// <param name="userId">Sepeti getirilecek kullanıcının ID'si</param>
        /// <returns>Kullanıcının sepeti, ürünleri ve resimleri ile birlikte</returns>
        public Basket CartByUserId(string userId)
        {
            using (var context = new DataContext())
            {
                return context.Baskets
                    .Include(i => i.BasketItems)
                    .ThenInclude(i => i.Product)
                    .ThenInclude(i => i.Images)
                    .FirstOrDefault(i => i.UserId == userId);
            }
        }

        /// <summary>
        /// Belirtilen sepet ID'sine sahip sepeti tamamen temizler
        /// Raw SQL kullanarak BasketItem tablosundan tüm kayıtları siler
        /// </summary>
        /// <param name="cartId">Temizlenecek sepetin ID'si</param>
        public void ClearFrommCart(string cartId)
        {
            using (var context = new DataContext())
            {
                var cmd = @"delete from BasketItem where BasketId=@p0";
                context.Database.ExecuteSqlRaw(cmd, cartId);
            }
        }

        /// <summary>
        /// Sepetten belirtilen ürünü kaldırır
        /// Raw SQL kullanarak BasketItem tablosundan belirli kaydı siler
        /// </summary>
        /// <param name="basketId">Sepet ID'si</param>
        /// <param name="productId">Sepetten kaldırılacak ürünün ID'si</param>
        public void DeleteFromCart(int basketId, int productId)
        {
            using (var context = new DataContext())
            {
                var cmd = @"delete from BasketItem where BasketId=@p0 and ProductId=@p1";
                context.Database.ExecuteSqlRaw(cmd, basketId, productId);
            }
        }

        /// <summary>
        /// Sepeti günceller
        /// </summary>
        /// <param name="entity">Güncellenecek sepet</param>
        /// <exception cref="ArgumentNullException">Entity null ise fırlatılır</exception>
        public override void Update(Basket entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Güncellenmek istenen basket boş!");

            using (var context = new DataContext())
            {
                context.Baskets.Update(entity);
                context.SaveChanges();
            }
        }
    }
}
