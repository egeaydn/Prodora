using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore
{
    public class EfCoreProductDal : EfCoreGenericRepository<Product, DataContext>, IProductDal
    {
        public EfCoreProductDal(IDbContextFactory<DataContext> contextFactory) : base(contextFactory) { }

        // Use identical filters for the result count and the page contents.
        private static IQueryable<Product> Filter(IQueryable<Product> products, string? category, string? search)
        {
            products = products.Where(p => !p.IsArchived);
            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
                products = products.Where(p => p.ProductCategory.Any(pc => pc.Category.Name == category));
            if (!string.IsNullOrWhiteSpace(search))
                products = products.Where(p => p.Name.Contains(search) || p.Brand.Contains(search));
            return products;
        }

        public int GetCountByDCategory(string? category, string? search = null)
        {
            using var context = _contextFactory.CreateDbContext();
            return Filter(context.Products, category, search).Count();
        }

        public Product GetProductDetails(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Products.Include(p => p.Images).Include(p => p.Variants)
                .Include(p => p.ProductCategory).ThenInclude(pc => pc.Category)
                .Include(p => p.Comments).FirstOrDefault(p => p.Id == id);
        }

        public List<Product> GetProductsByCategory(string? category, int page, int pageSize, string? search = null, string sort = "newest")
        {
            using var context = _contextFactory.CreateDbContext();
            var products = Filter(context.Products.AsNoTracking().Include(p => p.Images).Include(p => p.Variants)
                .Include(p => p.ProductCategory).ThenInclude(pc => pc.Category), category, search);
            var sorted = sort switch
            {
                "price-asc" => products.OrderBy(p => p.Price).ThenBy(p => p.Id),
                "price-desc" => products.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
                "name" => products.OrderBy(p => p.Name).ThenBy(p => p.Id),
                _ => products.OrderByDescending(p => p.Id)
            };
            return sorted.Skip((Math.Max(page, 1) - 1) * pageSize).Take(pageSize).ToList();
        }

        public void Update(Product entity, int[] categoryIds) => Update(entity, categoryIds, []);

        public List<string> Update(Product entity, int[] categoryIds, int[] removedImageIds) =>
            DeadlockRetry.Run(() => UpdateOnce(entity, categoryIds, removedImageIds));

        private List<string> UpdateOnce(Product entity, int[] categoryIds, int[] removedImageIds)
        {
            using var context = _contextFactory.CreateDbContext();
            using var transaction = context.Database.BeginTransaction();
            // Checkout uses the same product lock before snapshotting the catalog image.
            StockLedger.LockProducts(context, [entity.Id]);
            var product = context.Products.Include(p => p.ProductCategory).Include(p => p.Images).Include(p => p.Variants)
                .FirstOrDefault(p => p.Id == entity.Id);
            if (product == null) throw new StoreValidationException("Ürün bulunamadı.");
            var removedIds = removedImageIds.Distinct().ToHashSet();
            if (removedIds.Any(id => !product.Images.Any(i => i.Id == id)))
                throw new StoreValidationException("Görsel listesi değişmiş olabilir. Sayfayı yenileyip tekrar dene.");
            var removed = product.Images.Where(i => removedIds.Contains(i.Id)).ToList();
            context.Images.RemoveRange(removed);
            product.Name = entity.Name;
            product.Description = entity.Description;
            product.Price = entity.Price;
            product.Brand = entity.Brand;
            // With variants the product's stock is derived from them and managed on the variants page.
            if (product.Variants.Count == 0)
            {
                product.Stock = entity.Stock;
                product.StockQuantity = entity.StockQuantity;
            }
            var selectedIds = categoryIds.Distinct().ToHashSet();
            product.ProductCategory.RemoveAll(pc => !selectedIds.Contains(pc.CategoryId));
            foreach (var categoryId in selectedIds.Where(id => !product.ProductCategory.Any(pc => pc.CategoryId == id)))
                product.ProductCategory.Add(new ProductCategory { ProductId = entity.Id, CategoryId = categoryId });
            product.Images.AddRange(entity.Images.Where(image => image.Id == 0).Select(image => new Image { ImageUrl = image.ImageUrl }));
            context.SaveChanges();
            // A shared catalog image or an order snapshot must keep its file.
            var unused = removed.Select(i => i.ImageUrl).OfType<string>().Distinct()
                .Where(name => !context.Images.Any(i => i.ImageUrl == name)
                    && !context.Set<OrderItem>().Any(i => i.ProductImage == name)).ToList();
            transaction.Commit();
            return unused;
        }

        public override void Delete(Product entity)
        {
            using var context = _contextFactory.CreateDbContext();
            var product = context.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == entity.Id);
            if (product == null) return;
            product.IsArchived = true;
            context.SaveChanges();
        }

        public void Restore(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var product = context.Products.Find(id);
            if (product == null) return;
            product.IsArchived = false;
            context.SaveChanges();
        }

        public List<Product> GetAllIncludingArchived()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Products.AsNoTracking().Include(p => p.Images).Include(p => p.Variants).OrderByDescending(p => p.Id).ToList();
        }

        public override List<Product> GetAll(Expression<Func<Product, bool>> filter = null)
        {
            using var context = _contextFactory.CreateDbContext();
            var products = context.Products.AsNoTracking().Include(p => p.Images).Include(p => p.Variants).Where(p => !p.IsArchived);
            return (filter == null ? products : products.Where(filter)).OrderByDescending(p => p.Id).ToList();
        }
    }
}
