using Microsoft.Extensions.Caching.Memory;
using Prodora.Business.Abstract;
using Prodora.Entitys;

namespace Prodora.Business.Concrate;

/// <summary>
/// Caches the category list that every page header reads. Any change made through this service clears it;
/// the short expiry covers changes made by another instance.
/// </summary>
public class CachedCategoryServices(ICategoryServices inner, IMemoryCache cache) : ICategoryServices
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
    private const string Key = "categories:all";

    public List<Category> GetAll()
    {
        var categories = cache.GetOrCreate(Key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = Lifetime;
            return inner.GetAll();
        }) ?? new List<Category>();
        // Hand out a copy so one caller reordering or filtering the list cannot change what the next caller sees.
        return new List<Category>(categories);
    }

    public Category GetById(int id) => inner.GetById(id);
    public Category GetByWithProducts(int id) => inner.GetByWithProducts(id);
    public void Create(Category entity) { inner.Create(entity); cache.Remove(Key); }
    public void Update(Category entity) { inner.Update(entity); cache.Remove(Key); }
    public void Delete(Category entity) { inner.Delete(entity); cache.Remove(Key); }
    public void DeleteFromCategory(int categoryId, int productId) { inner.DeleteFromCategory(categoryId, productId); cache.Remove(Key); }
}
