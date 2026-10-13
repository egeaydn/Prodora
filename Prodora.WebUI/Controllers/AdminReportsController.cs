using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Reports;

namespace Prodora.WebUI.Controllers;

[Authorize(Roles = "admin")]
public class AdminReportsController(IOrderServices orders, IProductServices products) : Controller
{
    [HttpGet]
    public IActionResult Index(DateOnly? from, DateOnly? to)
    {
        var (start, end) = SalesReport.NormalizeRange(from, to, DateOnly.FromDateTime(DateTime.Today));
        ViewData["LowStock"] = LowStock.Find(products.GetAll());
        return View(SalesReport.Build(Load(start, end), start, end));
    }

    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult OrdersCsv(DateOnly? from, DateOnly? to)
    {
        var (start, end) = SalesReport.NormalizeRange(from, to, DateOnly.FromDateTime(DateTime.Today));
        // The byte order mark makes Excel read the Turkish characters as UTF-8.
        var bytes = new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(OrderCsv.Build(Load(start, end)))).ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"siparisler-{start:yyyy-MM-dd}-{end:yyyy-MM-dd}.csv");
    }

    private List<Prodora.Entitys.Order> Load(DateOnly start, DateOnly end) =>
        orders.GetOrdersBetween(start.ToDateTime(TimeOnly.MinValue), end.AddDays(1).ToDateTime(TimeOnly.MinValue));
}
