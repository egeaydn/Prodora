using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodora.Entitys
{
    [Table("ImagesTable")]
	public class Image
    {
		public int Id { get; set; }
		public string? ImageUrl { get; set; }
		public int ProductId { get; set; }
		public Product Product { get; set; }
	}
}
