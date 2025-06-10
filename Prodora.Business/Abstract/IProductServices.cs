using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.Business.Abstract
{
    public interface IProductServices
    {
		Product GetById(int id);
		List<Product> GetEProductByDivision(string division, int page, int pageSize);
		List<Product> GetAll();
		Product GetProductDetail(int id);
		void Create(Product entity);
		void Update(Product entity, int[] divisionIds);
		void Delete(Product entity);
		int GetCountByDivision(string division);
		public List<Product> GetProductsByPriceRange(decimal minPrice, decimal maxPrice);
	}
}
