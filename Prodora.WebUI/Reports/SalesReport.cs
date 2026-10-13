using Prodora.Entitys;

namespace Prodora.WebUI.Reports;

public sealed record DailySales(DateOnly Day, int Orders, decimal Revenue);

public sealed record TopProduct(int ProductId, string Name, int Quantity, decimal Revenue);

public sealed record SalesSummary(
    DateOnly From, DateOnly To, decimal Revenue, int OrderCount, decimal AverageOrder, decimal Discounts, int CancelledCount,
    IReadOnlyList<DailySales> Daily, IReadOnlyList<TopProduct> TopProducts);

public static class SalesReport
{
    public const int MaxDays = 366;

    /// <summary>An order is a sale once it is paid and not cancelled. Unpaid transfers and unresolved payments are not revenue yet.</summary>
    public static bool CountsAsSale(OrderStatus status) =>
        status is OrderStatus.Completed or OrderStatus.Processing or OrderStatus.Shipped or OrderStatus.Delivered;

    /// <summary>Clamps a requested range: default is the last 30 days, never reversed, never longer than a year.</summary>
    public static (DateOnly From, DateOnly To) NormalizeRange(DateOnly? from, DateOnly? to, DateOnly today)
    {
        var end = to ?? today;
        var start = from ?? end.AddDays(-29);
        if (start > end) (start, end) = (end, start);
        if (end.DayNumber - start.DayNumber + 1 > MaxDays) start = end.AddDays(-(MaxDays - 1));
        return (start, end);
    }

    public static SalesSummary Build(IEnumerable<Order> orders, DateOnly from, DateOnly to, int topCount = 5)
    {
        var inRange = orders.Where(o => DateOnly.FromDateTime(o.OrderDate) >= from && DateOnly.FromDateTime(o.OrderDate) <= to).ToList();
        var sales = inRange.Where(o => CountsAsSale(o.OrderEnums)).ToList();

        var perDay = sales.GroupBy(o => DateOnly.FromDateTime(o.OrderDate)).ToDictionary(g => g.Key, g => (Count: g.Count(), Revenue: g.Sum(o => o.Total)));
        var daily = new List<DailySales>();
        for (var day = from; day <= to; day = day.AddDays(1))
            daily.Add(perDay.TryGetValue(day, out var value) ? new(day, value.Count, value.Revenue) : new(day, 0, 0m));

        // Item revenue is before coupons: a coupon applies to the whole basket, not to one product.
        var top = sales.SelectMany(o => o.OrderItems)
            .GroupBy(i => i.ProductId)
            .Select(g => new TopProduct(g.Key, g.Select(i => i.ProductName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? "Ürün",
                g.Sum(i => i.Quantity), g.Sum(i => i.Price * i.Quantity)))
            .OrderByDescending(t => t.Revenue).ThenByDescending(t => t.Quantity).ThenBy(t => t.ProductId)
            .Take(topCount).ToList();

        var revenue = sales.Sum(o => o.Total);
        return new SalesSummary(from, to, revenue, sales.Count, sales.Count == 0 ? 0m : decimal.Round(revenue / sales.Count, 2),
            sales.Sum(o => o.DiscountAmount), inRange.Count(o => o.OrderEnums == OrderStatus.Cancelled), daily, top);
    }
}
