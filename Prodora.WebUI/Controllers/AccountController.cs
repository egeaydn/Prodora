using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prodora.WebUI.EmailServices;
using Prodora.WebUI.Extensions;
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

		public async Task<IActionResult> ConfirmEmail(string token,string userId)
		{
			if (userId == null || token == null )
			{
				TempData.Put("message", new ResultModels()
				{
					Title = "Hatalı Token",
					Message = "Hesap Onay Bilgileri Yanlış veya Kusurlu",
					Css = "danger"
				});
				return Redirect("~");
			}

			var user = await _userManager.FindByIdAsync(userId);

			if (user != null)
			{
				var result = await _userManager.ConfirmEmailAsync(user, token);//Email Confirmed  ı 1 yapıyoruz

				if (result.Succeeded)
				{

					TempData.Put("message", new ResultModels()
					{
						Title = "Hesap Oluşturuldu",
						Message = "Hesabınız Başarı ile Oluşturuldu",
						Css = "success"
					});

					return RedirectToAction("Login", "Account");
				}
			}

				
				TempData.Put("message", new ResultModels()
				{
					Title = "Hesap Oluşturulamadı",
					Message = "Hesabınız Oluşturulmadı Bir Hata Oluştu",
					Css = "danger"
				});							

			return View("~");

		}

		public IActionResult Login(string returnUrl = null)
		{
			return View(
				new LoginModel()
				{
					ReturnUrl = returnUrl
				}
			);
		}

		[HttpPost]

		public async Task<IActionResult> Login(LoginModel model)
		{
			ModelState.Remove("ReturnUrl");

			if (!ModelState.IsValid)
			{
				TempData.Put("message", new ResultModels()
				{
					Title = "Hatalı Giriş",
					Message = "Kullanıcı Adı veya Şifre Hatalı",
					Css = "danger"
				});
				
			}

			return View(model);
		}
	}
}
