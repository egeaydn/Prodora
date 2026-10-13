using NSubstitute;
using Prodora.Business.Concrate;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.Tests;

public class WishlistManagerTests
{
    private readonly IWishlistDal dal = Substitute.For<IWishlistDal>();

    [Fact]
    public void Toggle_reports_whether_the_product_is_now_saved()
    {
        dal.Toggle("user-1", 5).Returns(true, false);
        var manager = new WishlistManager(dal);

        Assert.True(manager.Toggle("user-1", 5));
        Assert.False(manager.Toggle("user-1", 5));
    }

    [Fact]
    public void Reads_are_scoped_to_the_given_customer()
    {
        dal.GetProductIds("user-1").Returns(new HashSet<int> { 1, 2 });
        dal.GetProducts("user-1").Returns(new List<Product> { new() { Id = 1 } });
        dal.GetProductIds("user-2").Returns(new HashSet<int>());
        var manager = new WishlistManager(dal);

        Assert.Equal(2, manager.GetProductIds("user-1").Count);
        Assert.Single(manager.GetProducts("user-1"));
        Assert.Empty(manager.GetProductIds("user-2"));
    }

    [Fact]
    public void Wishlist_size_limit_is_a_sane_positive_number() =>
        Assert.InRange(WishlistItem.MaxPerUser, 1, 1000);
}
