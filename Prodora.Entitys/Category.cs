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
		public string Name { get; set; }
		public List<ProductCategory> ProductCategories { get; set; }
		public Category()
		{
			ProductCategories  = new List<ProductCategory>();
		}
	}
}
