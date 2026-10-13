using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Prodora.Business.Abstract;
using Prodora.Business.Concrate;
using Prodora.Entitys;

namespace Prodora.Tests;

public class CachedCategoryServicesTests
{
    private readonly ICategoryServices inner = Substitute.For<ICategoryServices>();
    private readonly IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
    private readonly CachedCategoryServices service;

    public CachedCategoryServicesTests()
    {
        inner.GetAll().Returns(new List<Category> { new() { Id = 1, Name = "Shoes" } });
        service = new CachedCategoryServices(inner, cache);
    }

    [Fact]
    public void Repeated_reads_hit_the_database_once()
    {
        service.GetAll();
        service.GetAll();
        service.GetAll();

        inner.Received(1).GetAll();
    }

    [Fact]
    public void Callers_get_their_own_list_so_one_cannot_change_what_the_next_sees()
    {
        service.GetAll().Clear();

        Assert.Single(service.GetAll());
    }

    [Fact]
    public void Creating_a_category_clears_the_cache()
    {
        service.GetAll();
        service.Create(new Category { Name = "Bags" });
        service.GetAll();

        inner.Received(2).GetAll();
    }

    [Fact]
    public void Updating_deleting_and_unlinking_also_clear_the_cache()
    {
        var category = new Category { Id = 1 };

        service.GetAll(); service.Update(category); service.GetAll();
        service.Delete(category); service.GetAll();
        service.DeleteFromCategory(1, 2); service.GetAll();

        inner.Received(4).GetAll();
    }

    [Fact]
    public void Single_category_reads_are_not_cached()
    {
        service.GetById(1);
        service.GetById(1);

        inner.Received(2).GetById(1);
    }

    [Fact]
    public void A_failed_change_keeps_the_cache_it_did_not_touch()
    {
        service.GetAll();
        inner.When(i => i.Create(Arg.Any<Category>())).Do(_ => throw new InvalidOperationException("db down"));

        Assert.Throws<InvalidOperationException>(() => service.Create(new Category()));
        service.GetAll();

        inner.Received(1).GetAll();
    }
}
