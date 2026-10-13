using Prodora.Entitys;
using Prodora.WebUI.Reports;

namespace Prodora.Tests;

public class SalesReportTests
{
    private static readonly DateOnly Today = new(2026, 10, 13);

    private static Order Sale(int id, DateTime date, OrderStatus status, decimal price, int quantity = 1, decimal discount = 0, int productId = 1, string name = "Ürün")
        => new()
        {
            Id = id, OrderDate = date, OrderEnums = status, DiscountAmount = discount,
            OrderItems = [new OrderItem { ProductId = productId, ProductName = name, Price = price, Quantity = quantity }]
        };

    [Fact]
    public void Only_paid_and_not_cancelled_orders_count_as_sales()
    {
        var orders = new[]
        {
            Sale(1, new(2026, 10, 10, 9, 0, 0), OrderStatus.Completed, 100m),
            Sale(2, new(2026, 10, 10, 10, 0, 0), OrderStatus.Delivered, 50m),
            Sale(3, new(2026, 10, 10, 11, 0, 0), OrderStatus.Pending, 999m),
            Sale(4, new(2026, 10, 10, 12, 0, 0), OrderStatus.Cancelled, 999m),
            Sale(5, new(2026, 10, 10, 13, 0, 0), OrderStatus.PaymentUncertain, 999m),
            Sale(6, new(2026, 10, 10, 14, 0, 0), OrderStatus.PaymentFailed, 999m)
        };

        var report = SalesReport.Build(orders, new(2026, 10, 1), Today);

        Assert.Equal(2, report.OrderCount);
        Assert.Equal(150m, report.Revenue);
        Assert.Equal(75m, report.AverageOrder);
        Assert.Equal(1, report.CancelledCount);
    }

    [Fact]
    public void Revenue_is_after_coupons_and_discounts_are_totalled()
    {
        var report = SalesReport.Build([Sale(1, new(2026, 10, 12, 9, 0, 0), OrderStatus.Completed, 100m, quantity: 2, discount: 20m)], new(2026, 10, 1), Today);

        Assert.Equal(180m, report.Revenue);
        Assert.Equal(20m, report.Discounts);
    }

    [Fact]
    public void Daily_series_has_one_entry_per_day_including_empty_days()
    {
        var report = SalesReport.Build([Sale(1, new(2026, 10, 11, 9, 0, 0), OrderStatus.Completed, 40m)], new(2026, 10, 10), new(2026, 10, 13));

        Assert.Equal(4, report.Daily.Count);
        Assert.Equal(new[] { 0m, 40m, 0m, 0m }, report.Daily.Select(d => d.Revenue));
        Assert.Equal(new DateOnly(2026, 10, 10), report.Daily[0].Day);
    }

    [Fact]
    public void Range_ends_are_inclusive_and_outside_orders_are_ignored()
    {
        var orders = new[]
        {
            Sale(1, new(2026, 10, 9, 23, 59, 59), OrderStatus.Completed, 10m),
            Sale(2, new(2026, 10, 10, 0, 0, 0), OrderStatus.Completed, 20m),
            Sale(3, new(2026, 10, 13, 23, 59, 59), OrderStatus.Completed, 30m),
            Sale(4, new(2026, 10, 14, 0, 0, 0), OrderStatus.Completed, 40m)
        };

        Assert.Equal(50m, SalesReport.Build(orders, new(2026, 10, 10), new(2026, 10, 13)).Revenue);
    }

    [Fact]
    public void Empty_period_has_zero_everything_and_no_division_error()
    {
        var report = SalesReport.Build([], new(2026, 10, 1), Today);

        Assert.Equal((0m, 0, 0m), (report.Revenue, report.OrderCount, report.AverageOrder));
        Assert.Empty(report.TopProducts);
    }

    [Fact]
    public void Top_products_are_ranked_by_revenue_and_limited()
    {
        var orders = new List<Order>
        {
            Sale(1, new(2026, 10, 12, 9, 0, 0), OrderStatus.Completed, 10m, quantity: 5, productId: 1, name: "Ucuz"),
            Sale(2, new(2026, 10, 12, 10, 0, 0), OrderStatus.Completed, 100m, quantity: 1, productId: 2, name: "Pahalı"),
            Sale(3, new(2026, 10, 12, 11, 0, 0), OrderStatus.Completed, 10m, quantity: 3, productId: 1, name: "Ucuz"),
            Sale(4, new(2026, 10, 12, 12, 0, 0), OrderStatus.Cancelled, 5000m, productId: 3, name: "İptal")
        };

        var top = SalesReport.Build(orders, new(2026, 10, 1), Today, topCount: 1).TopProducts;

        Assert.Single(top);
        Assert.Equal(("Pahalı", 1, 100m), (top[0].Name, top[0].Quantity, top[0].Revenue));
        var all = SalesReport.Build(orders, new(2026, 10, 1), Today).TopProducts;
        Assert.Equal(new[] { "Pahalı", "Ucuz" }, all.Select(t => t.Name));
        Assert.Equal(80m, all[1].Revenue);
        Assert.Equal(8, all[1].Quantity);
    }

    [Fact]
    public void Default_range_is_the_last_thirty_days()
    {
        var (from, to) = SalesReport.NormalizeRange(null, null, Today);

        Assert.Equal(Today, to);
        Assert.Equal(Today.AddDays(-29), from);
    }

    [Fact]
    public void Reversed_range_is_swapped_and_long_range_is_clamped_to_a_year()
    {
        Assert.Equal((new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5)), SalesReport.NormalizeRange(new(2026, 10, 5), new(2026, 10, 1), Today));

        var (from, to) = SalesReport.NormalizeRange(new(2000, 1, 1), Today, Today);
        Assert.Equal(Today, to);
        Assert.Equal(SalesReport.MaxDays, to.DayNumber - from.DayNumber + 1);
    }
}

public class OrderCsvTests
{
    private static Order Row(string name = "Ege", string email = "ege@example.test", string phone = "05551234567") => new()
    {
        Id = 12, OrderDate = new(2026, 10, 12, 14, 30, 0), OrderEnums = OrderStatus.Shipped, PaymentEnum = OrderPayments.CreditCard,
        FirstName = name, LastName = "Aydın", Email = email, Phone = phone, City = "İstanbul", CouponCode = "SAVE10", TrackingNumber = "TR-1",
        DiscountAmount = 10m, OrderItems = [new OrderItem { Price = 55.5m, Quantity = 2 }]
    };

    [Fact]
    public void Header_and_one_row_are_semicolon_separated_with_turkish_decimals()
    {
        var lines = OrderCsv.Build([Row()]).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lines.Length);
        Assert.StartsWith("Siparis No;Tarih;", lines[0]);
        Assert.Equal("#000012;2026-10-12 14:30;Kargoya verildi;Kredi / banka kartı;Ege Aydın;ege@example.test;05551234567;İstanbul;2;111,00;10,00;101,00;SAVE10;TR-1", lines[1]);
    }

    [Theory]
    [InlineData("=HYPERLINK(\"http://evil\")")]
    [InlineData("+cmd|' /C calc'!A0")]
    [InlineData("-2+3")]
    [InlineData("@SUM(A1)")]
    public void Cells_that_a_spreadsheet_would_run_as_formulas_are_defused(string name)
    {
        var line = OrderCsv.Build([Row(name: name)]).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)[1];

        Assert.DoesNotContain(";" + name[0], line);
        Assert.DoesNotContain(";\"" + name[0], line);
        Assert.Contains("'" + name[0], line);
    }

    [Fact]
    public void Separators_quotes_and_newlines_inside_a_value_are_quoted()
    {
        var csv = OrderCsv.Build([Row(name: "A;B \"C\"\nD")]);

        Assert.Contains("\"A;B \"\"C\"\"\nD Aydın\"", csv);
    }

    [Fact]
    public void Rows_are_ordered_by_date_then_id()
    {
        var later = Row(); later.Id = 1; later.OrderDate = new(2026, 10, 13);
        var earlier = Row(); earlier.Id = 2; earlier.OrderDate = new(2026, 10, 1);

        var lines = OrderCsv.Build([later, earlier]).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.StartsWith("#000002", lines[1]);
        Assert.StartsWith("#000001", lines[2]);
    }

    [Fact]
    public void No_orders_still_produces_the_header() =>
        Assert.Single(OrderCsv.Build([]).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
}
