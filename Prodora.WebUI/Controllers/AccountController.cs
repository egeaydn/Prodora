using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prodora.WebUI.EmailServices;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers
{
	public class AccountController : Controller
	{

		private UserManager<IdentityUser> _userManager;
		private RoleManager<IdentityRole> _roleManager;

		public AccountController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_roleManager = roleManager;
			_userManager = userManager;
		}
		public IActionResult Regiser()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Register(RegisterPage model)
		{

			if (!ModelState.IsValid)
			{
				return View (model);
			}

			var user = new ApplicationUser()
			{
				UserName = model.UserName,
				Email = model.Email,
				FullName = model.FullName
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
				var callbackUrl = Url.Action("ConfirmEmail", "Account", new
				{
					userId = user.Id,
					token = code
				});

				string siteUrl = "https://localhost:5174";
				string activeUrl =$"{siteUrl}{callbackUrl}";

				// Send email with confirmation link

				string body = $@"
				<p>Merhaba {user.UserName},</p>
				<p>Hesabınızı onaylamak için aşağıdaki bağlantıya tıklayın:</p>
				<a href='{activeUrl}'>Hesabımı Onayla</a>";

				MailHelper.SendEmail(body, model.Email,"Prodora Admin Hesap Aktifleştirme Onayı");

				return RedirectToAction("Login", "Account");
			}

			return View(model);
		}
	}
}
