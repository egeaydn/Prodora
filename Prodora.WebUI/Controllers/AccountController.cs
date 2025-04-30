using Microsoft.AspNetCore.Mvc;

namespace Prodora.WebUI.Controllers
{
	public class AccountController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
