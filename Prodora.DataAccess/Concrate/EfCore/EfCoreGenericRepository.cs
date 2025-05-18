using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;

namespace Prodora.DataAccess.Concrate.EfCore
{
	public class EfCoreGenericRepository<T, TContext> : IRepository<T>where T : class where TContext : DbContext, new()
	{
		public void Create(T entity)
		{
			using (var context = new TContext())
			{
				context.Set<T>().Add(entity);
				context.SaveChanges();
			}
		}

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


		public virtual List<T> GetAll(Expression<Func<T, bool>> filter = null)
		{
			using (var context = new TContext())
			{
				return filter == null ? context.Set<T>().ToList() : context.Set<T>().Where(filter).ToList();
			}
		}

		public T GetById(int id)
		{
			using (var context = new TContext())
			{
				return context.Set<T>().Find(id);
			}
		}

		public virtual T GetOne(Expression<Func<T, bool>> filter = null)
		{
			using (var context = new TContext())
			{
				return context.Set<T>().FirstOrDefault(filter);
			}
		}

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
