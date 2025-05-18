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
		public IActionResult List(string categoryName , int page = 1)
		{
			const int pageSize = 5;

			var products = new ProductListModel()
			{
				PageInfo = new PageInfo()
				{
					TotalItems = _productServices.GetCountByDivision(categoryName),
					ItemsPerPage = pageSize,
					CurrentCategory = categoryName,
					CurrentPage = page
				},
				Products = _productServices.GetEProductByDivision(categoryName, page, pageSize)
			};

			return View(products);
		}
		
		public IActionResult Details(int? id)	
		{
			if (id == null)
			{
				return NotFound();
			}
			Product product = _productServices.GetEProductDetail(id.Value);
			if (product == null)
			{
				return NotFound();
			}
			return View(new ProductDetail()
			{
				Products = product,
				Categories = product.ProductCategory.Select(i => i.Category).ToList(),
			});
		}
	}
}
