using System.Globalization;
using System.Text;
using Prodora.Entitys;
using Prodora.WebUI.Orders;

namespace Prodora.WebUI.Reports;

/// <summary>Order export for spreadsheets: semicolon separated with Turkish decimals, which is what Turkish Excel opens correctly.</summary>
public static class OrderCsv
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");
    private static readonly string[] Header =
    {
        "Siparis No", "Tarih", "Durum", "Odeme", "Musteri", "E-posta", "Telefon", "Sehir", "Urun adedi",
        "Ara toplam", "Indirim", "Toplam", "Kupon", "Kargo takip no"
    };

    public static string Build(IEnumerable<Order> orders)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(';', Header.Select(Cell)));
        foreach (var order in orders.OrderBy(o => o.OrderDate).ThenBy(o => o.Id))
        {
            builder.AppendLine(string.Join(';', new[]
            {
                "#" + order.Id.ToString("D6"),
                order.OrderDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                OrderStatusDisplay.Label(order.OrderEnums),
                OrderStatusDisplay.PaymentLabel(order.PaymentEnum),
                $"{order.FirstName} {order.LastName}".Trim(),
                order.Email, order.Phone, order.City,
                order.OrderItems.Sum(i => i.Quantity).ToString(CultureInfo.InvariantCulture),
                Money(order.Subtotal), Money(order.DiscountAmount), Money(order.Total),
                order.CouponCode ?? "", order.TrackingNumber ?? ""
            }.Select(Cell)));
        }
        return builder.ToString();
    }

    private static string Money(decimal value) => value.ToString("0.00", Turkish);

    /// <summary>Quotes when needed, and defuses cells a spreadsheet would run as a formula (=, +, -, @).</summary>
    internal static string Cell(string? value)
    {
        var text = value ?? "";
        if (text.Length > 0 && text[0] is '=' or '+' or '-' or '@' or '\t' or '\r') text = "'" + text;
        return text.IndexOfAny(new[] { ';', '"', '\r', '\n' }) >= 0 ? "\"" + text.Replace("\"", "\"\"") + "\"" : text;
    }
}
