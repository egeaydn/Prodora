using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract
{
    public interface IProductDal : IRepository<Product>
	{
        int GetCountByDCategory(int categoryId);
		List<Product> GetProductsByCategory(int categoryId, int page, int pageSize);
		void Update (Product entity,int[] categoryIds);
		Product GetProductDetails(int id);
	}
}
