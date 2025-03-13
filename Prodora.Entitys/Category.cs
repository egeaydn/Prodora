using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodora.Entitys
{
    public class Category
    {
		public int Id { get; set; }
		public string CategoryName { get; set; }
		public List<ProductCategory> ProductCategorys { get; set; }
		public Category()
		{
			ProductCategorys = new List<ProductCategory>();
		}
	}
}
