using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Business.Abstract;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.Business.Concrate
{
	class CategoryManager : ICategoryServices
	{
		private ICategoryDal _categoryDal;

		public CategoryManager(ICategoryDal categoryDal)
		{
			_categoryDal = categoryDal;
		}

		public void Create(Category entity)
		{
			_categoryDal.Create(entity);
		}

		public void Delete(Category entity)
		{
			_categoryDal.Delete(entity);
		}

		public void DeleteFromCategory(int categoryId, int productId)
		{
			_categoryDal.DeleteCategory(categoryId, productId);
		}

		public List<Category> GetAll()
		{
			return _categoryDal.GetAll();
		}

		public Category GetById(int id)
		{
			return _categoryDal.GetById(id);
		}

		public Category GetByWithProducts(int id)
		{
			return _categoryDal.GetByProducts(id);
		}

		public void Update(Category entity)
		{
			_categoryDal.Update(entity);
		}
	}
}
