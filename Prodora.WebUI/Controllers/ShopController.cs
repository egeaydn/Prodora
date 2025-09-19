using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Models;
using Microsoft.AspNetCore.Identity;
using Prodora.WebUI.Identity;

namespace Prodora.WebUI.Controllers
{
	public class ShopController : Controller
	{
		IProductServices _productServices;
		ICommentServices _commentServices;
		private readonly UserManager<ApplicationUser> _userManager;

		public ShopController( IProductServices productServices, ICommentServices commentServices, UserManager<ApplicationUser> userManager)
		{
			_productServices = productServices;
			_commentServices = commentServices;
			_userManager = userManager;
		}

		[Route("products/{category?}")]
		public IActionResult List(string category, int page = 1)
		{
			const int pageSize = 6;

			// Kategori adını küçük harfe çeviriyoruz
			category = category?.ToLower();

			var products = new ProductListModel()
			{
				PageInfo = new PageInfo()
				{
					TotalItems = _productServices.GetCountByDivision(category),
					ItemsPerPage = pageSize,
					CurrentCategory = category,
					CurrentPage = page
				},
				Products = _productServices.GetEProductByDivision(category, page, pageSize)
			};

			return View(products);
		}
		
		public IActionResult Details(int? id)	
		{
			if (id == null)
			{
				return NotFound();
			}
			Product product = _productServices.GetProductDetail(id.Value);
			if (product == null)
			{
				return NotFound();
			}
			var relatedProducts = _productServices.GetEProductByDivision(product.ProductCategory.FirstOrDefault()?.Category.Name, page: 1, pageSize: 4)
								.Where(p => p.Id != id.Value)
								.ToList();

			// YORUM: Ürüne ait tüm yorumları getir
			var allComments = _commentServices.GetCommentByProductId(id.Value);
			
			// YORUM: Sadece aktif kullanıcıları olan yorumları filtrele
			var activeComments = new List<Comment>();
			var users = new Dictionary<string, string>();
			
			foreach (var comment in allComments)
			{
				var user = _userManager.FindByIdAsync(comment.UserId).Result;
				if (user != null) // Kullanıcı hala aktifse
				{
					activeComments.Add(comment);
					users[comment.UserId] = user.FullName ?? "Anonim Kullanıcı";
				}
				// Eğer user null ise (silinmişse), bu yorumu activeComments'e ekleme
			}
			
			ViewBag.Usernames = users;

			// YORUM: ProductDetail modelini view'a gönderiyoruz
			return View(new ProductDetail()
			{
				Products = product,
				Categories = product.ProductCategory.Select(i => i.Category).ToList(),
				Comments = activeComments, // Sadece aktif kullanıcıları olan yorumları gönder
				RelatedProducts = relatedProducts
			});
		}
	}
}
