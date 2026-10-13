using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.WebUI.Seo;

namespace Prodora.WebUI.Controllers;

public class SeoController(IProductServices products, ICategoryServices categories) : Controller
{
    private static readonly XNamespace Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

    [HttpGet("sitemap.xml"), ResponseCache(Duration = 3600)]
    public IActionResult Sitemap()
    {
        var urls = new List<string> { "/", "/products" };
        urls.AddRange(categories.GetAll().Select(c => Url.Action("List", "Shop", new { category = c.Name })!));
        urls.AddRange(products.GetAll().Where(p => !p.IsArchived)
            .Select(p => Url.RouteUrl("shopDetails", new { id = p.Id, slug = Slugger.Slugify(p.Name) })!));

        var document = new XDocument(new XDeclaration("1.0", "utf-8", null),
            new XElement(Ns + "urlset", urls.Distinct().Select(path => new XElement(Ns + "url", new XElement(Ns + "loc", SiteUrl.Absolute(HttpContext, path))))));
        return Content(document.Declaration + Environment.NewLine + document.Root, "application/xml", Encoding.UTF8);
    }

    [HttpGet("robots.txt"), ResponseCache(Duration = 3600)]
    public IActionResult Robots()
    {
        var text = string.Join('\n',
            "User-agent: *",
            "Disallow: /Admin",
            "Disallow: /AdminOrders",
            "Disallow: /AdminCoupons",
            "Disallow: /PaymentReview",
            "Disallow: /Basket",
            "Disallow: /Orders",
            "Disallow: /Account",
            "Disallow: /orders",
            "Disallow: /checkout",
            "Disallow: /admin",
            "Sitemap: " + SiteUrl.Absolute(HttpContext, "/sitemap.xml"),
            "");
        return Content(text, "text/plain", Encoding.UTF8);
    }
}
