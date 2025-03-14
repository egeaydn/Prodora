using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract
{
    public interface ICategoryDal : IRepository<Category>
	{
        void DeleteCategory(int categoryId,int productId);
        Category GetByeProducts(int categoryId);
	}
}
