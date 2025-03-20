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
	class ProductManager : IProductServices
	{
		private IProductDal _productDal;
		public void Create(Product entity)
		{
			_productDal.Create(entity);
		}

		public void Delete(Product entity)
		{
			_productDal.Delete(entity);
		}

		public List<Product> GetAll()
		{
			return _productDal.GetAll();
		}

		public Product GetById(int id)
		{
			return _productDal.GetById(id);
		}

		public int GetCountByDivision(string division)
		{
			return _productDal.GetCountByDCategory(division);
		}

		public List<Product> GetEProductByDivision(string division, int page, int pageSize)
		{
			return _productDal.GetProductsByCategory(division, page, pageSize);
		}

		public Product GetEProductDetail(int id)
		{
			return _productDal.GetProductDetails(id);
		}

		public List<Product> GetProductsByPriceRange(decimal minPrice, decimal maxPrice)
		{
			return _productDal.GetProductsByPriceRange(minPrice, maxPrice);
		}

		public void Update(Product entity, int[] divisionIds)
		{
			_productDal.Update(entity, divisionIds);
		}
	}
}
