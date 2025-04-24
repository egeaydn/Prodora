using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers
{
	public class AdminController : Controller
	{
		private IProductServices _productServices;	
		private ICategoryServices _categoryServices;
		private UserManager<IdentityUser> _userManager;
		private RoleManager<IdentityRole> _roleManager;

		public AdminController(IProductServices productServices, ICategoryServices categoryServices,UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_categoryServices = categoryServices;
			_productServices = productServices;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		[Route("admin/products")]
		public IActionResult ProductList()
		{
			return View(
				new ProductListModel()
				{
					Products = _productServices.GetAll()
				}
			);
		}

		public IActionResult CreateProduct()
		{
			var category = _categoryServices.GetAll();
			ViewBag.Category = category.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });

			return View(new ProductModel());
		}
		[HttpPost]
		public async Task<IActionResult> CreateProduct(ProductModel model, List<IFormFile>files)
		{
			ModelState.Remove("SelectedCategories");

			if (ModelState.IsValid)
			{
				if (int.Parse(model.CategoryId) == -1)
				{
					ModelState.AddModelError("", "Please select a category");
					ViewBag.Category = _categoryServices.GetAll()
					.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });

					return View(model);
				}

				var entity = new Product()
				{
					Name = model.Name,
					Description = model.Description,
					Price = model.Price,
				};

				if (files.Count < 4 || files == null)
				{
					ModelState.AddModelError("", "Please select at least 4 images");
					ViewBag.Category = _categoryServices.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });

					return View(model);
				}

				foreach (var file in files)
				{
					Image image = new Image();
					image.ImageUrl = file.FileName;

					entity.Images.Add(image);
				}
			}
			return View();
		}
	}
}
