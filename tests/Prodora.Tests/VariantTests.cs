using Prodora.Entitys;
using Prodora.WebUI.Reports;

namespace Prodora.Tests;

public class VariantRulesTests
{
    [Theory]
    [InlineData("M", "M")]
    [InlineData("  Siyah   /  42 ", "Siyah / 42")]
    [InlineData("Büyük\tBoy", "Büyük Boy")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Names_are_trimmed_and_inner_whitespace_collapsed(string? input, string expected) =>
        Assert.Equal(expected, VariantRules.NormalizeName(input));

    [Fact]
    public void A_normal_variant_is_valid() => Assert.Null(VariantRules.Validate("M", 5));

    [Theory]
    [InlineData("")]
    [InlineData("0123456789012345678901234567890123456789012345678901234567890")]
    public void Empty_or_overlong_names_are_rejected(string name) => Assert.NotNull(VariantRules.Validate(name, 1));

    [Fact]
    public void Control_characters_are_rejected() => Assert.NotNull(VariantRules.Validate("M\u0007", 1));

    [Theory]
    [InlineData(-1)]
    [InlineData(100001)]
    public void Quantity_must_be_within_bounds(int quantity) => Assert.NotNull(VariantRules.Validate("M", quantity));

    [Fact]
    public void Zero_stock_is_a_valid_starting_point() => Assert.Null(VariantRules.Validate("M", 0));

    [Fact]
    public void Product_is_bought_by_choice_only_when_a_variant_is_active()
    {
        Assert.False(VariantRules.HasChoices(new Product()));
        Assert.False(VariantRules.HasChoices(new Product { Variants = [new() { IsActive = false }] }));
        Assert.True(VariantRules.HasChoices(new Product { Variants = [new() { IsActive = false }, new() { IsActive = true }] }));
    }

    [Fact]
    public void Product_with_unset_variants_list_is_handled()
    {
        var product = new Product { Variants = null! };
        Assert.False(VariantRules.HasChoices(product));
    }

    [Fact]
    public void In_stock_means_an_active_variant_with_units_left()
    {
        Assert.True(VariantRules.AnyInStock([new() { IsActive = true, StockQuantity = 1 }]));
        Assert.False(VariantRules.AnyInStock([new() { IsActive = true, StockQuantity = 0 }]));
        Assert.False(VariantRules.AnyInStock([new() { IsActive = false, StockQuantity = 9 }]));
        Assert.False(VariantRules.AnyInStock([]));
    }
}

public class OrderItemNameTests
{
    [Fact]
    public void Variant_name_is_part_of_the_display_name() =>
        Assert.Equal("Ceket · M", new OrderItem { ProductName = "Ceket", VariantName = "M" }.DisplayName);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Without_a_variant_only_the_product_name_shows(string? variant) =>
        Assert.Equal("Ceket", new OrderItem { ProductName = "Ceket", VariantName = variant }.DisplayName);

    [Fact]
    public void A_missing_product_name_falls_back_to_a_generic_label() =>
        Assert.Equal("Ürün", new OrderItem().DisplayName);
}

public class LowStockTests
{
    [Fact]
    public void Variants_are_listed_individually_and_emptiest_first()
    {
        var shirt = new Product
        {
            Id = 1, Name = "Gömlek",
            Variants = [new() { Name = "S", StockQuantity = 3, IsActive = true }, new() { Name = "M", StockQuantity = 0, IsActive = true }, new() { Name = "L", StockQuantity = 40, IsActive = true }]
        };
        var mug = new Product { Id = 2, Name = "Kupa", StockQuantity = 2 };

        var lines = LowStock.Find([shirt, mug]);

        Assert.Equal(new[] { "Gömlek · M", "Kupa", "Gömlek · S" }, lines.Select(l => l.Name));
        Assert.Equal(new[] { true, false, true }, lines.Select(l => l.IsVariant));
    }

    [Fact]
    public void Inactive_variants_archived_products_and_untracked_products_are_ignored()
    {
        var products = new[]
        {
            new Product { Id = 1, Name = "A", Variants = [new() { Name = "S", StockQuantity = 0, IsActive = false }, new() { Name = "M", StockQuantity = 50, IsActive = true }] },
            new Product { Id = 2, Name = "B", StockQuantity = 1, IsArchived = true },
            new Product { Id = 3, Name = "C", StockQuantity = null }
        };

        Assert.Empty(LowStock.Find(products));
    }
}
