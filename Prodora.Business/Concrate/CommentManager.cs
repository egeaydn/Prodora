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
	public class CommentManager : ICommentServices
	{
		private ICommentDal _commentDal;
		public CommentManager(ICommentDal commentDal)
		{
			_commentDal = commentDal;
		}
		public void Create(Comment entity)
		{
			_commentDal.Create(entity);
		}

		public void Delete(Comment entity)
		{
			_commentDal.Delete(entity);
		}

		public double GetAvaragesRaiting(int productId)
		{
			return _commentDal.GetAverageRating(productId);
		}

		public Comment GetById(int id)
		{
			return _commentDal.GetById(id);
		}

		public List<Comment> GetCommentByProductId(int productId)
		{
			return _commentDal.GetCommentsByProductId(productId);
		}

		public void Update(Comment entity)
		{
			_commentDal.Update(entity);
		}
	}
}
