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
        List<Product> GetAllIncludingArchived();
        void Restore(int id);
		Product GetById(int id);
		List<Product> GetEProductByDivision(string? division, int page, int pageSize, string? search = null, string sort = "newest");
		List<Product> GetAll();
		Product GetProductDetail(int id);
		void Create(Product entity);
		void Update(Product entity, int[] divisionIds);
		void Delete(Product entity);
		int GetCountByDivision(string? division, string? search = null);
		public List<Product> GetProductsByPriceRange(decimal minPrice, decimal maxPrice);
	}
}
