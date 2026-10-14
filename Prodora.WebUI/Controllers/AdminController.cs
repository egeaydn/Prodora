using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Audit;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly IProductServices _products;
        private readonly ICategoryServices _categories;
        private readonly Images.ProductImageStorage _images;
        private readonly ILogger<AdminController> _logger;
        private readonly IAuditLog _audit;

        public AdminController(IProductServices products, ICategoryServices categories, Images.ProductImageStorage images, IAuditLog audit, ILogger<AdminController> logger)
        {
            _products = products;
            _categories = categories;
            _images = images;
            _logger = logger;
            _audit = audit;
        }

        public IActionResult ProductList() => View(new ProductListModel { Products = _products.GetAllIncludingArchived() });

        [HttpPost]
        public IActionResult RestoreProduct(int productId)
        {
            _products.Restore(productId);
            _audit.Record("product.restore", $"Ürün #{productId}");
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
            var product = new Product { Name = model.Name, Description = model.Description, Price = model.Price, Brand = model.Brand };
            StockPolicy.Apply(product, model.StockQuantity, model.Stock);
            var uploaded = new List<Image>();
            try
            {
                uploaded = await _images.SaveAsync(files);
                product.Images = uploaded;
                product.ProductCategory.Add(new ProductCategory { CategoryId = categoryId });
                _products.Create(product);
            }
            catch (Exception exception)
            {
                _images.Delete(uploaded.Select(i => i.ImageUrl));
                _logger.LogError("Product creation failed; error type {ErrorType}", exception.GetType().Name);
                ModelState.AddModelError("", "Ürün kaydedilemedi. Görselleri yeniden seçip tekrar dene.");
                PopulateCategories();
                return View(model);
            }
            _audit.Record("product.create", $"Ürün #{product.Id} {product.Name}", $"fiyat {product.Price:0.00} TL, stok {(product.StockQuantity?.ToString() ?? (product.Stock ? "var" : "yok"))}");
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
                Price = product.Price, Brand = product.Brand, Stock = product.Stock, StockQuantity = product.StockQuantity,
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
            if (model.RemovedImageIds.Any(id => !product.Images.Any(i => i.Id == id)))
                ModelState.AddModelError(nameof(model.RemovedImageIds), "Kaldırmak istediğin görselleri yeniden kontrol et.");
            if (!ModelState.IsValid)
            {
                model.Images = product.Images;
                model.SelectedCategories = categories.Where(c => categoryIds.Contains(c.Id)).ToList();
                PopulateCategories();
                return View(model);
            }
            product.Name = model.Name; product.Description = model.Description; product.Price = model.Price;
            product.Brand = model.Brand;
            StockPolicy.Apply(product, model.StockQuantity, model.Stock);
            var uploaded = new List<Image>();
            List<string> unused;
            try
            {
                uploaded = await _images.SaveAsync(files);
                product.Images.AddRange(uploaded);
                unused = _products.Update(product, categoryIds, model.RemovedImageIds);
            }
            catch (Exception exception)
            {
                _images.Delete(uploaded.Select(i => i.ImageUrl));
                _logger.LogError("Product update failed; error type {ErrorType}", exception.GetType().Name);
                ModelState.AddModelError("", "Değişiklikler kaydedilemedi. Sayfayı kontrol edip yeni görselleri yeniden seç.");
                model.Images = product.Images.Where(i => i.Id != 0).ToList();
                model.SelectedCategories = categories.Where(c => categoryIds.Contains(c.Id)).ToList();
                PopulateCategories();
                return View(model);
            }
            _images.Delete(unused);
            _audit.Record("product.update", $"Ürün #{product.Id} {product.Name}", $"fiyat {product.Price:0.00} TL, stok {(product.StockQuantity?.ToString() ?? (product.Stock ? "var" : "yok"))}");
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

        [HttpPost]
        public IActionResult DeleteProduct(int productId)
        {
            var product = _products.GetProductDetail(productId);
            if (product != null) { _products.Delete(product); _audit.Record("product.archive", $"Ürün #{product.Id} {product.Name}"); }
            return RedirectToAction(nameof(ProductList));
        }

        public IActionResult CreateCategory() => View(new CategoryModel());

        [HttpPost]
        public IActionResult CreateCategory(CategoryModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name)) ModelState.AddModelError(nameof(model.Name), "Kategori adı gerekli.");
            if (!ModelState.IsValid) return View(model);
            _categories.Create(new Category { Name = model.Name.Trim() });
            _audit.Record("category.create", $"Kategori {model.Name.Trim()}");
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
            _audit.Record("category.update", $"Kategori #{category.Id}", $"Yeni ad: {category.Name}");
            return RedirectToAction(nameof(CategoryList));
        }

        [HttpPost]
        public IActionResult DeleteCategory(int categoryId)
        {
            var category = _categories.GetById(categoryId);
            if (category != null) { _categories.Delete(category); _audit.Record("category.delete", $"Kategori #{category.Id} {category.Name}"); }
            return RedirectToAction(nameof(CategoryList));
        }
    }
}
