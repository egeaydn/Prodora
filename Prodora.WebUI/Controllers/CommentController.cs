using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Prodora.Business.Abstract;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Models;

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

		public IActionResult Create (CommentModel commentModel,int? productId)
		{
			ModelState.Remove("UserId");
			if (ModelState.IsValid)
			{
				if (productId is null)
				{
					return BadRequest("Model state is not valid.");
				}

				Product product = _productServices.GetById(productId.Value);

				if (product is null)
				{
					return NotFound("Product not found.");
				}

				Comment comment = new Comment
				{
					CommentText = commentModel.Text.Trim('\n').Trim(' '),
					ProductId = productId.Value,
					UserId = _userManager.GetUserId(User) ?? "0",
					Raitings = commentModel.Raiting,
					CreatedAt = DateTime.Now
				};

				_commentServices.Create(comment);

				return Json(new {result = true});
			}
			return View(commentModel);
		}

		public IActionResult GetComments(int? productId)
		{
			var comments = _commentServices.GetCommentByProductId(productId.Value);

			var users = new Dictionary<string, string>();
			foreach (var comment in comments)
			{
				if (!users.ContainsKey(comment.UserId))
				{
					var user = _userManager.FindByIdAsync(comment.UserId).Result;
					users[comment.UserId] = user?.FullName ?? "Unknown User";
				}
			}
			ViewBag.Usernames = users;
			return PartialView("_PartialComment", comments);
		}

		public IActionResult Delete(int? id)
		{
			if (id == null)
			{
				return BadRequest("Comment ID is required.");
			}

			Comment comment = _commentServices.GetById(id.Value);

			if (comment == null)
			{
				return NotFound("Comment not found.");
			}

			_commentServices.Delete(comment);

			return Json(new { result = true });
		}
	}
}
