using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers
{
    public class ShopController : Controller
    {
        private readonly IProductServices _productServices;
        private readonly ICommentServices _commentServices;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShopController(IProductServices productServices, ICommentServices commentServices, UserManager<ApplicationUser> userManager)
        {
            _productServices = productServices;
            _commentServices = commentServices;
            _userManager = userManager;
        }

        [HttpGet("products/{category?}")]
        public IActionResult List(string? category, int page = 1, string? q = null, string sort = "newest")
        {
            const int pageSize = 12;
            category = category?.Trim();
            q = q?.Trim();
            if (q?.Length > 100) q = q[..100];
            if (sort is not ("newest" or "price-asc" or "price-desc" or "name")) sort = "newest";
            var count = _productServices.GetCountByDivision(category, q);
            page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(count / (double)pageSize)));
            return View(new ProductListModel
            {
                SearchTerm = q,
                Sort = sort,
                PageInfo = new PageInfo { TotalItems = count, ItemsPerPage = pageSize, CurrentCategory = category, CurrentPage = page },
                Products = _productServices.GetEProductByDivision(category, page, pageSize, q, sort)
            });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var product = _productServices.GetProductDetail(id.Value);
            if (product == null || product.IsArchived) return NotFound();
            var related = _productServices.GetEProductByDivision(product.ProductCategory.FirstOrDefault()?.Category.Name, 1, 5)
                .Where(p => p.Id != id.Value).Take(4).ToList();
            var comments = _commentServices.GetCommentByProductId(id.Value);
            var users = new Dictionary<string, string>();
            foreach (var userId in comments.Select(c => c.UserId).Distinct())
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null) users[userId] = user.FullName ?? "Prodora kullanıcısı";
            }
            ViewBag.Usernames = users;
            return View(new ProductDetail
            {
                Products = product,
                Categories = product.ProductCategory.Select(pc => pc.Category).ToList(),
                Comments = comments.Where(c => users.ContainsKey(c.UserId)).ToList(),
                RelatedProducts = related
            });
        }
    }
}
