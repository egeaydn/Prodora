using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prodora.Entitys;

namespace Prodora.Business.Abstract
{
	public interface ICommentServices
	{
		Comment GetById(int id);
		void Create(Comment entity);
		void Update(Comment entity);
		void Delete(Comment entity);
		double GetAvaragesRaiting(int productId);
		List<Comment>GetCommentByProductId(int productId);
	}
}
