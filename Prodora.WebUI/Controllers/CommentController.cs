using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Identity;

namespace Prodora.WebUI.Controllers
{
	public class CommentController : Controller
	{
		private ICommentServices _commentServices;
		private IProductServices _productServices;
		private UserManager<ApplicationUser> _userManager;
		public CommentController(ICommentServices commentServices, IProductServices productServices, UserManager<ApplicationUser> userManager)
		{
			_commentServices = commentServices;
			_productServices = productServices;
			_userManager = userManager;
		}

		public IActionResult ShowProductsComment(int? id)
		{
			if (id == null)
			{
				return BadRequest("User is not authenticated.");
			}

			Product product = _productServices.GetEProductDetail(id.Value);

			if (product == null)
			{
				return NotFound("Product not found.");
			}

			var users = new Dictionary<string, string>();
			foreach (var comment in product.Comments)
			{
				var user = _userManager.FindByIdAsync(comment.UserId).Result;
				users[comment.UserId] = user?.FullName ?? "Unknown User";
			}

			ViewBag.Usernames = users;


			return PartialView("_PartialComment", product.Comments);
		}
	}
}
