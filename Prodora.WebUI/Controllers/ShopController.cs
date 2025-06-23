using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers
{
	public class ShopController : Controller
	{
		IProductServices _productServices;

		public ShopController( IProductServices productServices)
		{
			_productServices = productServices;
		}

		[Route("products/{category?}")]
		public IActionResult List(string category, int page = 1)
		{
			const int pageSize = 5;

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
			return View(new ProductDetail()
			{
				Products = product,
				Categories = product.ProductCategory.Select(i => i.Category).ToList(),
				Comments = product.Comments,
				RelatedProducts = relatedProducts
			});
		}
	}
}
