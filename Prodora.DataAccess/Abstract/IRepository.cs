using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Prodora.DataAccess.Abstract
{
    /// <summary>
    /// Generic repository pattern için temel interface
    /// Tüm entity'ler için ortak CRUD operasyonlarını tanımlar
    /// </summary>
    /// <typeparam name="T">Repository'nin çalışacağı entity tipi</typeparam>
    public interface IRepository<T>
    {
        /// <summary>
        /// Belirtilen ID'ye sahip entity'yi veritabanından getirir
        /// </summary>
        /// <param name="id">Aranacak entity'nin ID'si</param>
        /// <returns>Bulunan entity, bulunamazsa null</returns>
        T GetById(int id);

        /// <summary>
        /// Belirtilen filtreye uyan ilk entity'yi getirir
        /// </summary>
        /// <param name="filter">Entity'leri filtrelemek için lambda expression</param>
        /// <returns>Filtreye uyan ilk entity, bulunamazsa null</returns>
        T GetOne(Expression<Func<T, bool>> filter=null);

        /// <summary>
        /// Tüm entity'leri veya belirtilen filtreye uyan entity'leri getirir
        /// </summary>
        /// <param name="filter">Entity'leri filtrelemek için lambda expression</param>
        /// <returns>Filtreye uyan entity'lerin listesi</returns>
        List<T> GetAll(Expression<Func<T, bool>> filter = null);

        /// <summary>
        /// Yeni bir entity'yi veritabanına ekler
        /// </summary>
        /// <param name="entity">Eklenecek entity</param>
        void Create(T entity);

        /// <summary>
        /// Mevcut bir entity'yi günceller
        /// </summary>
        /// <param name="entity">Güncellenecek entity</param>
        void Update(T entity);

        /// <summary>
        /// Belirtilen entity'yi veritabanından siler
        /// </summary>
        /// <param name="entity">Silinecek entity</param>
        void Delete(T entity);
    }
}
