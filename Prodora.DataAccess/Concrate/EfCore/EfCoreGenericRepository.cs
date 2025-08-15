using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;

namespace Prodora.DataAccess.Concrate.EfCore
{
    /// <summary>
    /// Entity Framework Core kullanarak generic repository pattern implementasyonu
    /// Tüm entity'ler için ortak CRUD operasyonlarını sağlar
    /// </summary>
    /// <typeparam name="T">Repository'nin çalışacağı entity tipi</typeparam>
    /// <typeparam name="TContext">Kullanılacak DbContext tipi</typeparam>
    public class EfCoreGenericRepository<T, TContext> : IRepository<T> where T : class where TContext : DbContext, new()
    {
        /// <summary>
        /// Yeni bir entity'yi veritabanına ekler
        /// </summary>
        /// <param name="entity">Eklenecek entity</param>
        public void Create(T entity)
        {
            using (var context = new TContext())
            {
                context.Set<T>().Add(entity);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Belirtilen entity'yi veritabanından siler
        /// </summary>
        /// <param name="entity">Silinecek entity</param>
        /// <exception cref="ArgumentNullException">Entity null ise fırlatılır</exception>
        public virtual void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Silinmek istenen entity null olamaz.");

            using (var context = new TContext())
            {
                context.Set<T>().Remove(entity);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Tüm entity'leri veya belirtilen filtreye uyan entity'leri getirir
        /// </summary>
        /// <param name="filter">Entity'leri filtrelemek için lambda expression</param>
        /// <returns>Filtreye uyan entity'lerin listesi</returns>
        public virtual List<T> GetAll(Expression<Func<T, bool>> filter = null)
        {
            using (var context = new TContext())
            {
                return filter == null ? context.Set<T>().ToList() : context.Set<T>().Where(filter).ToList();
            }
        }

        /// <summary>
        /// Belirtilen ID'ye sahip entity'yi veritabanından getirir
        /// </summary>
        /// <param name="id">Aranacak entity'nin ID'si</param>
        /// <returns>Bulunan entity, bulunamazsa null</returns>
        public T GetById(int id)
        {
            using (var context = new TContext())
            {
                return context.Set<T>().Find(id);
            }
        }

        /// <summary>
        /// Belirtilen filtreye uyan ilk entity'yi getirir
        /// </summary>
        /// <param name="filter">Entity'leri filtrelemek için lambda expression</param>
        /// <returns>Filtreye uyan ilk entity, bulunamazsa null</returns>
        public virtual T GetOne(Expression<Func<T, bool>> filter = null)
        {
            using (var context = new TContext())
            {
                return context.Set<T>().FirstOrDefault(filter);
            }
        }

        /// <summary>
        /// Mevcut bir entity'yi günceller
        /// </summary>
        /// <param name="entity">Güncellenecek entity</param>
        public virtual void Update(T entity)
        {
            using (var context = new TContext())
            {
                context.Entry(entity).State = EntityState.Modified;
                context.SaveChanges();
            }
        }
    }
}
