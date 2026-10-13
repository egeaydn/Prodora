using Prodora.Entitys;

namespace Prodora.Tests;

public class StockPolicyTests
{
    [Fact]
    public void A_quantity_turns_tracking_on_and_stock_follows_it()
    {
        var product = new Product { Stock = false };

        StockPolicy.Apply(product, 7, inStockWhenUntracked: false);

        Assert.Equal(7, product.StockQuantity);
        Assert.True(product.Stock);
    }

    [Fact]
    public void Zero_quantity_means_out_of_stock_whatever_the_flag_says()
    {
        var product = new Product { Stock = true };

        StockPolicy.Apply(product, 0, inStockWhenUntracked: true);

        Assert.Equal(0, product.StockQuantity);
        Assert.False(product.Stock);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void A_blank_quantity_turns_tracking_off_and_the_flag_decides(bool flag)
    {
        var product = new Product { StockQuantity = 9, Stock = !flag };

        StockPolicy.Apply(product, null, inStockWhenUntracked: flag);

        Assert.Null(product.StockQuantity);
        Assert.Equal(flag, product.Stock);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(100000, true)]
    [InlineData(100001, false)]
    public void Quantity_must_be_within_bounds(int quantity, bool valid) =>
        Assert.Equal(valid, StockPolicy.ValidateQuantity(quantity) == null);

    [Fact]
    public void A_missing_quantity_is_always_valid() => Assert.Null(StockPolicy.ValidateQuantity(null));

    [Theory]
    [InlineData(null, false)]
    [InlineData(100, false)]
    [InlineData(6, false)]
    [InlineData(5, true)]
    [InlineData(1, true)]
    [InlineData(0, true)]
    public void Low_stock_starts_at_the_threshold_and_includes_sold_out(int? quantity, bool low) =>
        Assert.Equal(low, StockPolicy.IsLow(new Product { StockQuantity = quantity }));

    [Fact]
    public void Archived_products_never_raise_a_low_stock_warning() =>
        Assert.False(StockPolicy.IsLow(new Product { StockQuantity = 1, IsArchived = true }));

    [Theory]
    [InlineData(null, 99)]
    [InlineData(500, 99)]
    [InlineData(99, 99)]
    [InlineData(3, 3)]
    [InlineData(0, 0)]
    public void A_basket_line_is_capped_by_what_is_left(int? quantity, int expected) =>
        Assert.Equal(expected, StockPolicy.MaxPurchasable(new Product { StockQuantity = quantity }));
}
