using Microsoft.AspNetCore.Mvc;

namespace Prodora.WebUI.Controllers
{
	public class ToolsController : Controller
	{
		public IActionResult About()
		{
			return View();
		}

		public IActionResult Contact()
		{
			return View();
		}
		public IActionResult Privacy()
		{
			return View();
		}
		public IActionResult Terms()
		{
			return View();
		}
		public IActionResult Faq()
		{
			return View();
		}
		public IActionResult Help()
		{
			return View();
		}
		public IActionResult Sitemap()
		{
			return View();
		}
		public IActionResult Support()
		{
			return RedirectToAction(nameof(Contact));
		}
		public IActionResult Legal()
		{
			return RedirectToAction(nameof(Terms));
		}
		public IActionResult Cookies()
		{
			return RedirectToAction(nameof(Privacy));
		}
		public IActionResult Accessibility()
		{
			return View();
		}
		public IActionResult Quote()
		{
			var quotes = new[]
			{
				"Devam et, pes etme!",
				"Kod yaz, dünya değişsin.",
				"Başarısızlık, başarıya giden bir adımdır.",
				"Bugün yazdığın kod, yarının ürünüdür."
			};

			var random = new Random();
			ViewBag.RandomQuote = quotes[random.Next(quotes.Length)];
			return View();
		}

	}
}
