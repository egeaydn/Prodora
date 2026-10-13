using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodora.DataAccess.Abstract;

namespace Prodora.WebUI.Controllers;

[Authorize(Roles = "admin")]
public class AdminAuditController(IAuditDal audit) : Controller
{
    private const int PageSize = 50;
    private static readonly string[] Areas = ["order", "coupon", "product", "variant", "category", "payment"];

    [HttpGet]
    public IActionResult Index(string? area, int page = 1)
    {
        area = Areas.Contains(area) ? area : null;
        var total = audit.Count(area);
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)));
        ViewData["Area"] = area;
        ViewData["Page"] = page;
        ViewData["Pages"] = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        ViewData["Areas"] = Areas;
        return View(audit.Query(area, page, PageSize));
    }
}
