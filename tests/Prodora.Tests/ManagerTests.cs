using System.Linq.Expressions;
using NSubstitute;
using Prodora.Business.Concrate;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.Tests;

public class ProductManagerTests
{
    private readonly IProductDal dal = Substitute.For<IProductDal>();

    [Fact]
    public void Price_range_filter_is_inclusive_on_both_ends()
    {
        Expression<Func<Product, bool>>? captured = null;
        dal.GetAll(Arg.Do<Expression<Func<Product, bool>>>(f => captured = f)).Returns(new List<Product>());

        new ProductManager(dal).GetProductsByPriceRange(10m, 20m);

        var filter = captured!.Compile();
        Assert.False(filter(new Product { Price = 9.99m }));
        Assert.True(filter(new Product { Price = 10m }));
        Assert.True(filter(new Product { Price = 20m }));
        Assert.False(filter(new Product { Price = 20.01m }));
    }

    [Fact]
    public void Listing_passes_paging_search_and_sort_through_unchanged()
    {
        var expected = new List<Product> { new() { Id = 1 } };
        dal.GetProductsByCategory("shoes", 2, 12, "red", "price").Returns(expected);

        Assert.Same(expected, new ProductManager(dal).GetEProductByDivision("shoes", 2, 12, "red", "price"));
    }

    [Fact]
    public void Update_keeps_the_selected_categories()
    {
        var product = new Product { Id = 3 };

        new ProductManager(dal).Update(product, new[] { 1, 2 });

        dal.Received(1).Update(product, Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 1, 2 })));
    }
}

public class BasketManagerTests
{
    private readonly IBasketDal dal = Substitute.For<IBasketDal>();

    [Fact]
    public void Removing_an_item_uses_the_users_own_basket_id()
    {
        dal.CartByUserId("user-1").Returns(new Basket { Id = 42, UserId = "user-1" });

        new BasketManager(dal).DeleteFromBasket("user-1", 5);

        dal.Received(1).DeleteFromCart(42, 5);
    }

    [Fact]
    public void Removing_an_item_without_a_basket_does_nothing()
    {
        dal.CartByUserId("user-1").Returns((Basket)null!);

        new BasketManager(dal).DeleteFromBasket("user-1", 5);

        dal.DidNotReceiveWithAnyArgs().DeleteFromCart(default, default);
    }

    [Fact]
    public void Adding_an_item_is_delegated_with_the_requested_quantity()
    {
        new BasketManager(dal).AddToBasket("user-1", 5, 3);

        dal.Received(1).AddItem("user-1", 5, 3);
    }
}

public class OrderManagerTests
{
    private readonly IOrderDal dal = Substitute.For<IOrderDal>();

    [Fact]
    public void Status_change_returns_the_order_and_its_previous_status()
    {
        var order = new Order { Id = 4, OrderEnums = OrderStatus.Processing };
        dal.SetStatus(4, OrderStatus.Shipped).Returns((order, OrderStatus.Processing));

        var (updated, previous) = new OrderManager(dal).SetStatus(4, OrderStatus.Shipped);

        Assert.Same(order, updated);
        Assert.Equal(OrderStatus.Processing, previous);
    }

    [Fact]
    public void Store_orders_are_filtered_by_the_requested_bucket()
    {
        var review = new List<Order> { new() { Id = 1 } };
        dal.GetStoreOrders("review").Returns(review);

        Assert.Same(review, new OrderManager(dal).GetStoreOrders("review"));
    }

    [Fact]
    public void A_users_orders_come_from_that_users_id()
    {
        new OrderManager(dal).GetOrders("user-1");

        dal.Received(1).GetOrders("user-1");
    }
}
