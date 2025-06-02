using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodora.Entitys
{
	public class Comment
	{
		public int Id { get; set; }
		public string CommentText { get; set; }
		public DateTime CreatedAt { get; set; }
		public Product Product { get; set; }
		public string UserId { get; set; }
		public int ProductId { get; set; }
		public int Raiting { get; set; }

	}
}
