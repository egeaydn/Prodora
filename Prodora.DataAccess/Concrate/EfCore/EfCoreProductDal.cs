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
            return context.Products.Include(p => p.Images)
                .Include(p => p.ProductCategory).ThenInclude(pc => pc.Category)
                .Include(p => p.Comments).FirstOrDefault(p => p.Id == id);
        }

        public List<Product> GetProductsByCategory(string? category, int page, int pageSize, string? search = null, string sort = "newest")
        {
            using var context = _contextFactory.CreateDbContext();
            var products = Filter(context.Products.AsNoTracking().Include(p => p.Images)
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

        public void Update(Product entity, int[] categoryIds)
        {
            using var context = _contextFactory.CreateDbContext();
            var product = context.Products.Include(p => p.ProductCategory).Include(p => p.Images)
                .FirstOrDefault(p => p.Id == entity.Id);
            if (product == null) return;
            product.Name = entity.Name;
            product.Description = entity.Description;
            product.Price = entity.Price;
            product.Brand = entity.Brand;
            product.Stock = entity.Stock;
            var selectedIds = categoryIds.Distinct().ToHashSet();
            product.ProductCategory.RemoveAll(pc => !selectedIds.Contains(pc.CategoryId));
            foreach (var categoryId in selectedIds.Where(id => !product.ProductCategory.Any(pc => pc.CategoryId == id)))
                product.ProductCategory.Add(new ProductCategory { ProductId = entity.Id, CategoryId = categoryId });
            product.Images.AddRange(entity.Images.Where(image => image.Id == 0));
            context.SaveChanges();
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
            return context.Products.AsNoTracking().Include(p => p.Images).OrderByDescending(p => p.Id).ToList();
        }

        public override List<Product> GetAll(Expression<Func<Product, bool>> filter = null)
        {
            using var context = _contextFactory.CreateDbContext();
            var products = context.Products.AsNoTracking().Include(p => p.Images).Where(p => !p.IsArchived);
            return (filter == null ? products : products.Where(filter)).OrderByDescending(p => p.Id).ToList();
        }
    }
}
