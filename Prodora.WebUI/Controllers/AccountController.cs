using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Prodora.WebUI.EmailServices;
using Prodora.WebUI.Extensions;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers
{
	public class AccountController : Controller
	{

		private UserManager<ApplicationUser> _userManager;
		private SignInManager<ApplicationUser> _signInManager;

		public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
		{
			_signInManager = signInManager;
			_userManager = userManager;
		}
		public IActionResult Register()
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
					Title = "Giriş Bilgileri",
					Message = "Bilgileriniz Hatalıdır",
					Css = "danger"
				});

				return View(model);
			}

			var user = await _userManager.FindByEmailAsync(model.Email);

			if (user is null)
			{
				ModelState.AddModelError("", "Bu email adresi ile kayıtlı kullanıcı bulunamadı");
				return View(model);
			}

			var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, true);

			if (result.Succeeded)
			{
				return Redirect(model.ReturnUrl ?? "~/");
			}
			if (result.IsLockedOut)
			{
				TempData.Put("message", new ResultModels()
				{
					Title = "Hesap Kilitlendi",
					Message = "Hesabınız geçici olarak kilitlenmiştir. Lütfen biraz sonra tekrar deneyin.",
					Css = "danger"
				});
				return View(model);
			}

			ModelState.AddModelError("", "Email veya şifre hatalı");

			return View(model);
		}

		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			TempData.Put("message", new ResultModels()
			{
				Title = "Çıkış Yapıldı",
				Message = "Başarı ile Çıkış Yapıldı",
				Css = "success"
			});
			return RedirectToAction("Index", "Home");
		}

		/*
			buraya forgotpassword , reset password ve manage işlemleri yapılması 
			tanısında kararsız kalındı şuanlık herhangi bir şey yok
			ama ileride yapılabilir veya düşünülebilir ama yapılmamamasının temel sebebi şu 
		    Bu Kısım sadece yani Account Kısmı sadece Adminlerde gözükecek
			Admin Girşlerinde Kullanılacak Yani Aslında Kullanıcnın Burayla Bir işi 
			olmayacak adminler şifrelerinin unutmamalrı gerekecek eğer 
			böyle bir sıkıntı ile karşılaşırsak bu kısım işte ozaman eklenecek
		 */



	}
}
