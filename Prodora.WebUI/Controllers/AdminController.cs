﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers
{
	public class AdminController : Controller
	{
		private IProductServices _productServices;	
		private ICategoryServices _categoryServices;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AdminController(IProductServices productServices, ICategoryServices categoryServices,UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
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
					Brand = model.Brand,
					Stock = model.Stock
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

					var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img", file.FileName);

					using (var sream = new FileStream(path,FileMode.Create))
					{
						await file.CopyToAsync(sream);
					}
				}

				entity.ProductCategory = new List<ProductCategory> { new ProductCategory {CategoryId = int.Parse(model.CategoryId), ProductId = entity.Id } };
				_productServices.Create(entity);
				return RedirectToAction("ProductList");
			}
			ViewBag.Category = _categoryServices.GetAll()
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
			return View(model);
		}
		
		public  IActionResult EditProduct(int id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var entity = _productServices.GetById(id);
			if (entity == null)
			{
				return NotFound();
			}

			var firstCategory = entity.ProductCategory?.FirstOrDefault();

			var model = new ProductModel()
			{
				Id = entity.Id,
				Name = entity.Name,
				Description = entity.Description,
				Price = entity.Price,
				CategoryId = entity.ProductCategory?.FirstOrDefault()?.CategoryId.ToString() ?? "-1",
				Images = entity.Images ?? new List<Image>()
			};



			ViewBag.Categories = _categoryServices.GetAll();
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> EditProduct(ProductModel model, List<IFormFile> files, int[] categoryIds)
		{
			var entity = _productServices.GetById(model.Id);

			if (entity == null)
			{
				return NotFound();
			}

			entity.Name = model.Name;
			entity.Description = model.Description;
			entity.Price = model.Price;
			entity.Images = model.Images;

			foreach (var file in files)
			{
				if (file != null && file.Length > 0)
				{
					var extension = Path.GetExtension(file.FileName);
					var fileName = Guid.NewGuid().ToString() + extension;
					var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img");

					if (!Directory.Exists(folderPath))
					{
						Directory.CreateDirectory(folderPath);
					}

					var filePath = Path.Combine(folderPath, fileName);

					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await file.CopyToAsync(stream);
					}

					// Eğer ürünün image listesi varsa:
					entity.Images.Add(new Image
					{
						ImageUrl = fileName,
						ProductId = entity.Id
					});
				}
			}

			_productServices.Update(entity, categoryIds);


			return RedirectToAction("ProductList");
		}
		[HttpPost]
		public IActionResult DeleteProduct(int productId)
		{
			var product = _productServices.GetById(productId);

			if (product != null)
			{
				_productServices.Delete(product);
			}

			return RedirectToAction("ProductList");
		}

		public IActionResult CategoryList()
		{
			return View(new CategoryListModel()
			{
				Categories = _categoryServices.GetAll()
			});
		}

		public IActionResult EditCategory(int? id)
		{
			if (id == null)
			{
				return NotFound(); // veya BadRequest
			}

			var entity = _categoryServices.GetByWithProducts(id.Value);
			if (entity == null)
			{
				return NotFound(); // Bu da kritik!
			}

			return View(
				new CategoryModel()
				{
					Id = entity.Id,
					Name = entity.Name,
					Products = entity.ProductCategories.Select(i => i.Product).ToList()
				}
			);
		}


		[HttpPost]
		public IActionResult EditCategory(CategoryModel model)
		{
			if (!ModelState.IsValid)
			{
				// Hataları logla veya geri döndür
				var errors = ModelState.Values.SelectMany(v => v.Errors);
				return View(model); // Sayfayı aynı şekilde geri döndür ki hatalar görünsün
			}

			var entity = _categoryServices.GetById(model.Id);

			if (entity == null)
			{
				return NotFound();
			}

			entity.Name = model.Name;
			_categoryServices.Update(entity);
			return RedirectToAction("CategoryList");
		}


		[HttpPost]
		public IActionResult DeleteCategory(int categoryId)
		{
			var entity = _categoryServices.GetById(categoryId);
			if (entity != null)
			{
				_categoryServices.Delete(entity);
			}
			else
			{
				// Hata mesajı döndür veya logla
			    ModelState.AddModelError("", "Category not found");
			}
			return RedirectToAction("CategoryList");
		}

		public IActionResult CreateCategory()
		{
			return View(new CategoryModel());
		}

		[HttpPost]

		public IActionResult CreateCategory(CategoryModel model)
		{
			var entity = new Category()
			{
				Name = model.Name
			};

			_categoryServices.Create(entity);
			return RedirectToAction("CategoryList");
		}

	}
}
