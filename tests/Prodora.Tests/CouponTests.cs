using NSubstitute;
using Prodora.Business.Concrate;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.Tests;

public class CouponRulesTests
{
    private static readonly DateTime Now = new(2026, 10, 13, 12, 0, 0);

    private static Coupon Percent(decimal value = 10) => new() { Id = 1, Code = "SAVE10", Kind = CouponKind.Percent, Value = value };

    [Fact]
    public void Percent_coupon_takes_the_percentage_off_the_subtotal() =>
        Assert.Equal(25m, CouponRules.Evaluate(Percent(10), 250m, Now, false).Discount);

    [Fact]
    public void Percent_discount_is_rounded_to_kurus_away_from_zero() =>
        Assert.Equal(1.67m, CouponRules.Evaluate(Percent(10), 16.65m, Now, false).Discount);

    [Fact]
    public void Fixed_coupon_takes_the_fixed_amount_off()
    {
        var coupon = new Coupon { Code = "FIVE", Kind = CouponKind.FixedAmount, Value = 5m };
        Assert.Equal(5m, CouponRules.Evaluate(coupon, 100m, Now, false).Discount);
    }

    [Fact]
    public void Discount_never_takes_the_total_below_one_kurus()
    {
        var coupon = new Coupon { Code = "BIG", Kind = CouponKind.FixedAmount, Value = 500m };
        var check = CouponRules.Evaluate(coupon, 20m, Now, false);
        Assert.True(check.IsValid);
        Assert.Equal(19.99m, check.Discount);
    }

    [Fact]
    public void Missing_or_inactive_coupon_is_invalid()
    {
        Assert.False(CouponRules.Evaluate(null, 100m, Now, false).IsValid);
        var off = Percent(); off.IsActive = false;
        Assert.False(CouponRules.Evaluate(off, 100m, Now, false).IsValid);
    }

    [Fact]
    public void Coupon_expires_at_its_end_time()
    {
        var coupon = Percent(); coupon.ExpiresAt = Now;
        Assert.False(CouponRules.Evaluate(coupon, 100m, Now, false).IsValid);
        Assert.True(CouponRules.Evaluate(coupon, 100m, Now.AddMinutes(-1), false).IsValid);
    }

    [Fact]
    public void Used_up_coupon_is_invalid()
    {
        var coupon = Percent(); coupon.MaxRedemptions = 3; coupon.RedemptionCount = 3;
        Assert.False(CouponRules.Evaluate(coupon, 100m, Now, false).IsValid);
        coupon.RedemptionCount = 2;
        Assert.True(CouponRules.Evaluate(coupon, 100m, Now, false).IsValid);
    }

    [Fact]
    public void Same_customer_cannot_use_a_coupon_twice() =>
        Assert.False(CouponRules.Evaluate(Percent(), 100m, Now, userAlreadyRedeemed: true).IsValid);

    [Fact]
    public void Minimum_basket_total_is_enforced_inclusively()
    {
        var coupon = Percent(); coupon.MinSubtotal = 100m;
        Assert.False(CouponRules.Evaluate(coupon, 99.99m, Now, false).IsValid);
        Assert.True(CouponRules.Evaluate(coupon, 100m, Now, false).IsValid);
    }

    [Theory]
    [InlineData("save10", "SAVE10")]
    [InlineData("  yaz-25_a ", "YAZ-25_A")]
    public void Codes_are_trimmed_and_upper_cased(string input, string expected) =>
        Assert.Equal(expected, CouponRules.NormalizeCode(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a b")]
    [InlineData("indirim%")]
    [InlineData("'; DROP TABLE Coupons;--")]
    public void Unsafe_or_empty_codes_are_rejected(string? input) => Assert.Null(CouponRules.NormalizeCode(input));

    [Fact]
    public void Code_length_is_limited_to_thirty_two() =>
        Assert.Null(CouponRules.NormalizeCode(new string('A', 33)));

    [Theory]
    [InlineData(CouponKind.Percent, 0, false)]
    [InlineData(CouponKind.Percent, 1, true)]
    [InlineData(CouponKind.Percent, 90, true)]
    [InlineData(CouponKind.Percent, 91, false)]
    [InlineData(CouponKind.Percent, 12.5, false)]
    [InlineData(CouponKind.FixedAmount, 0, false)]
    [InlineData(CouponKind.FixedAmount, 10.5, true)]
    [InlineData(CouponKind.FixedAmount, 10.555, false)]
    public void Definition_values_are_validated(CouponKind kind, double value, bool valid)
    {
        var coupon = new Coupon { Code = "OK", Kind = kind, Value = (decimal)value };
        Assert.Equal(valid, CouponRules.ValidateDefinition(coupon) == null);
    }

    [Fact]
    public void Negative_minimum_and_zero_limit_are_rejected()
    {
        Assert.NotNull(CouponRules.ValidateDefinition(new Coupon { Code = "A", Kind = CouponKind.Percent, Value = 5, MinSubtotal = -1 }));
        Assert.NotNull(CouponRules.ValidateDefinition(new Coupon { Code = "A", Kind = CouponKind.Percent, Value = 5, MaxRedemptions = 0 }));
    }
}

public class OrderTotalsTests
{
    [Fact]
    public void Total_is_subtotal_minus_discount()
    {
        var order = new Order
        {
            DiscountAmount = 10m,
            OrderItems = [new OrderItem { Price = 50m, Quantity = 2 }, new OrderItem { Price = 5m, Quantity = 1 }]
        };
        Assert.Equal(105m, order.Subtotal);
        Assert.Equal(95m, order.Total);
    }
}

public class CouponManagerTests
{
    private readonly ICouponDal dal = Substitute.For<ICouponDal>();

    [Fact]
    public void Preview_looks_the_code_up_case_insensitively_and_checks_the_customers_history()
    {
        dal.GetByCode("SAVE10").Returns(new Coupon { Id = 7, Code = "SAVE10", Kind = CouponKind.Percent, Value = 10 });
        dal.HasRedeemed(7, "user-1").Returns(false);

        var check = new CouponManager(dal).Preview(" save10 ", "user-1", 200m);

        Assert.True(check.IsValid);
        Assert.Equal(20m, check.Discount);
    }

    [Fact]
    public void Preview_rejects_a_coupon_the_customer_already_used()
    {
        dal.GetByCode("SAVE10").Returns(new Coupon { Id = 7, Code = "SAVE10", Kind = CouponKind.Percent, Value = 10 });
        dal.HasRedeemed(7, "user-1").Returns(true);

        Assert.False(new CouponManager(dal).Preview("SAVE10", "user-1", 200m).IsValid);
    }

    [Fact]
    public void Preview_of_an_unknown_or_malformed_code_is_invalid_without_touching_the_database()
    {
        var manager = new CouponManager(dal);

        Assert.False(manager.Preview("NOPE", "user-1", 200m).IsValid);
        Assert.False(manager.Preview("'; --", "user-1", 200m).IsValid);
        dal.DidNotReceive().GetByCode("'; --");
    }

    [Fact]
    public void Empty_code_means_no_coupon_and_no_error() =>
        Assert.Null(new CouponManager(dal).Preview("  ", "user-1", 200m).Error);
}
