using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.Business.Abstract
{
    public interface ICategoryServices
    {
		Category GetById(int id);
		Category GetByWithProducts(int id);
		List<Category> GetAll();
		void Create(Category entity);
		void Update(Category entity);
		void Delete(Category entity);
		void DeleteFromCategory(int categoryId, int productId);
	}
}
