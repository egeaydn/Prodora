using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly IProductServices _products;
        private readonly ICategoryServices _categories;
        private readonly IWebHostEnvironment _environment;

        public AdminController(IProductServices products, ICategoryServices categories, IWebHostEnvironment environment)
        {
            _products = products;
            _categories = categories;
            _environment = environment;
        }

        public IActionResult ProductList() => View(new ProductListModel { Products = _products.GetAllIncludingArchived() });

        [HttpPost]
        public IActionResult RestoreProduct(int productId)
        {
            _products.Restore(productId);
            return RedirectToAction(nameof(ProductList));
        }
        public IActionResult CategoryList() => View(new CategoryListModel { Categories = _categories.GetAll() });

        private void PopulateCategories()
        {
            var categories = _categories.GetAll();
            ViewBag.Categories = categories;
            ViewBag.Category = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
        }

        public IActionResult CreateProduct()
        {
            PopulateCategories();
            return View(new ProductModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductModel model, List<IFormFile> files)
        {
            if (!int.TryParse(model.CategoryId, out var categoryId) || _categories.GetById(categoryId) == null)
                ModelState.AddModelError(nameof(model.CategoryId), "Bir kategori seçin.");
            if (files.Count < 4) ModelState.AddModelError("files", "En az 4 ürün görseli seçin.");
            ValidateImages(files);
            if (!ModelState.IsValid) { PopulateCategories(); return View(model); }
            var product = new Product { Name = model.Name, Description = model.Description, Price = model.Price, Brand = model.Brand, Stock = model.Stock };
            product.Images = await SaveImages(files);
            product.ProductCategory.Add(new ProductCategory { CategoryId = categoryId });
            _products.Create(product);
            return RedirectToAction(nameof(ProductList));
        }

        public IActionResult EditProduct(int id)
        {
            var product = _products.GetProductDetail(id);
            if (product == null) return NotFound();
            PopulateCategories();
            return View(new ProductModel
            {
                Id = product.Id, Name = product.Name, Description = product.Description,
                Price = product.Price, Brand = product.Brand, Stock = product.Stock,
                Images = product.Images, SelectedCategories = product.ProductCategory.Select(pc => pc.Category).ToList(),
                CategoryId = product.ProductCategory.FirstOrDefault()?.CategoryId.ToString() ?? "-1"
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductModel model, List<IFormFile> files, int[] categoryIds)
        {
            var product = _products.GetProductDetail(model.Id);
            if (product == null) return NotFound();
            var categories = _categories.GetAll();
            if (categoryIds.Length == 0 || categoryIds.Any(id => !categories.Any(c => c.Id == id)))
                ModelState.AddModelError("categoryIds", "En az bir geçerli kategori seçin.");
            ValidateImages(files);
            if (!ModelState.IsValid)
            {
                model.Images = product.Images;
                model.SelectedCategories = categories.Where(c => categoryIds.Contains(c.Id)).ToList();
                PopulateCategories();
                return View(model);
            }
            product.Name = model.Name; product.Description = model.Description; product.Price = model.Price;
            product.Brand = model.Brand; product.Stock = model.Stock;
            product.Images.AddRange(await SaveImages(files));
            _products.Update(product, categoryIds);
            return RedirectToAction(nameof(ProductList));
        }

        private void ValidateImages(List<IFormFile> files)
        {
            var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (files.Count > 12) ModelState.AddModelError("files", "Bir defada en fazla 12 görsel ekleyin.");
            foreach (var file in files)
            {
                if (!extensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()) || file.Length == 0 || file.Length > 5 * 1024 * 1024)
                    ModelState.AddModelError("files", "JPG, PNG veya WebP biçiminde, 5 MB'dan küçük görseller seçin.");
            }
        }

        private async Task<List<Image>> SaveImages(List<IFormFile> files)
        {
            var images = new List<Image>();
            var folder = Path.Combine(_environment.WebRootPath, "img");
            Directory.CreateDirectory(folder);
            foreach (var file in files)
            {
                var name = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
                await using var stream = new FileStream(Path.Combine(folder, name), FileMode.CreateNew);
                await file.CopyToAsync(stream);
                images.Add(new Image { ImageUrl = name });
            }
            return images;
        }

        [HttpPost]
        public IActionResult DeleteProduct(int productId)
        {
            var product = _products.GetProductDetail(productId);
            if (product != null) _products.Delete(product);
            return RedirectToAction(nameof(ProductList));
        }

        public IActionResult CreateCategory() => View(new CategoryModel());

        [HttpPost]
        public IActionResult CreateCategory(CategoryModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name)) ModelState.AddModelError(nameof(model.Name), "Kategori adı gerekli.");
            if (!ModelState.IsValid) return View(model);
            _categories.Create(new Category { Name = model.Name.Trim() });
            return RedirectToAction(nameof(CategoryList));
        }

        public IActionResult EditCategory(int id)
        {
            var category = _categories.GetByWithProducts(id);
            if (category == null) return NotFound();
            return View(new CategoryModel { Id = category.Id, Name = category.Name, Products = category.ProductCategories.Select(pc => pc.Product).ToList() });
        }

        [HttpPost]
        public IActionResult EditCategory(CategoryModel model)
        {
            var category = _categories.GetByWithProducts(model.Id);
            if (category == null) return NotFound();
            if (string.IsNullOrWhiteSpace(model.Name)) ModelState.AddModelError(nameof(model.Name), "Kategori adı gerekli.");
            if (!ModelState.IsValid) { model.Products = category.ProductCategories.Select(pc => pc.Product).ToList(); return View(model); }
            category.Name = model.Name.Trim();
            _categories.Update(category);
            return RedirectToAction(nameof(CategoryList));
        }

        [HttpPost]
        public IActionResult DeleteCategory(int categoryId)
        {
            var category = _categories.GetById(categoryId);
            if (category != null) _categories.Delete(category);
            return RedirectToAction(nameof(CategoryList));
        }
    }
}
