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

		public IActionResult ShowProductComments(int? id)
		{
			if (id == null)
			{
				return BadRequest("User is not authenticated.");
			}

			Product product = _productServices.GetProductDetail(id.Value);

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
		[HttpPost]
		public IActionResult Create(CommentModel commentModel)
		{
			try
			{
				if (ModelState.IsValid)
				{
					if (commentModel.ProductId is null)
					{
						return Json(new { result = false, message = "ProductId is required." });
					}

					Product product = _productServices.GetById(commentModel.ProductId.Value);

					if (product is null)
					{
						return Json(new { result = false, message = "Product not found." });
					}

					Comment comment = new Comment
					{
						CommentText = commentModel.Text.Trim('\n').Trim(' '),
						ProductId = commentModel.ProductId.Value,
						UserId = _userManager.GetUserId(User) ?? "0",
						Raitings = commentModel.Raiting,
						CreatedAt = DateTime.Now
					};

					_commentServices.Create(comment);

					return Json(new { result = true });
				}
				// ModelState hatalarını detaylı döndür
				var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
				return Json(new { result = false, message = "ModelState Invalid", errors });
			}
			catch (Exception ex)
			{
				return Json(new { result = false, message = ex.Message, stack = ex.StackTrace });
			}
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
		//Belirli bir kullanıcıya ait yorumları getir
		public IActionResult GetCommentsByUser(string userId)
		{
			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest("User ID is required.");
			}

			var comments = _commentServices.GetCommentsByUserId(userId);
			return PartialView("_PartialComment", comments);
		}
		//Belirli ürün adına göre yorumları getir
		public IActionResult GetCommentsByProductName(string productName)
		{
			if (string.IsNullOrEmpty(productName))
			{
				return BadRequest("Product name is required.");
			}

			var comments = _commentServices.GetCommentsByProductName(productName);
			return PartialView("_PartialComment", comments);
		}
		//Belirli kullanıcı adına göre yorumları getir
		public IActionResult GetCommentsByUserName(string userName)
		{
			if (string.IsNullOrEmpty(userName))
			{
				return BadRequest("User name is required.");
			}

			var comments = _commentServices.GetCommentsByUserName(userName);
			return PartialView("_PartialComment", comments);
		}

		// Belirli marka adına göre ürün yorumlarını getir
		public IActionResult GetCommentsByBrand(string brandName)
		{
			if (string.IsNullOrEmpty(brandName))
			{
				return BadRequest("Brand name is required.");
			}

			var comments = _commentServices.GetCommentsByProductBrand(brandName);
			return PartialView("_PartialComment", comments);
		}
		
		//Belirli fiyat aralığında yorumları getir
		public IActionResult GetCommentsByPriceRange(decimal min, decimal max)
		{
			if (min < 0 || max < 0 || min > max)
			{
				return BadRequest("Invalid price range.");
			}

			var comments = _commentServices.GetCommentsByProductPriceRange(min, max);
			return PartialView("_PartialComment", comments);
		}
		//Belirli puan (rating) değerine göre yorumları getir
		public IActionResult GetCommentsByRating(int rating)
		{
			var comments = _commentServices.GetCommentsByProductRating(rating);
			return PartialView("_PartialComment", comments);
		}
		//Belirli tarih aralığındaki yorumları getir
		public IActionResult GetCommentsByDateRange(DateTime startDate, DateTime endDate)
		{
			if (startDate > endDate)
			{
				return BadRequest("Invalid date range.");
			}

			var comments = _commentServices.GetCommentsByDateRange(startDate, endDate);
			return PartialView("_PartialComment", comments);
		}

		//Bir ürünün ortalama puanını getir
		public IActionResult GetAverageRating(int productId)
		{
			double averageRating = _commentServices.GetAverageRating(productId);
			return Json(new { rating = averageRating });
		}

	}
}
