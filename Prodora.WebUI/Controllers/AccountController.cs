using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Prodora.Business.Abstract;
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
		private IBasketServices _basketServices;

		public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager , IBasketServices basketServices)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_basketServices = basketServices;
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

				string siteUrl = "https://localhost:7136";
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
					_basketServices.InitialBasket(userId);

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

		//böyle bir sıkıntı ile karşılaşıldığı için kullanıcı kayıt olma kısmı ekleyeceğiz bunun için managede bu işe dahil
		//olacak ve hatta forgot password ve reset password işlemleri de yapılacak

		public IActionResult AccessDenied()
		{
			TempData.Put("message", new ResultModels()
			{
				Title = "Erişim Engellendi",
				Message = "Bu sayfaya erişim izniniz yok.",
				Css = "danger"
			});
			return View();
		}

		public async Task<IActionResult> Manage()
		{
			var user = await _userManager.GetUserAsync(User);

			if (user == null)
			{
				TempData.Put("message", new ResultModels()
				{
					Title = "Kullanıcı Bulunamadı",
					Message = "Kullanıcı bilgileri bulunamadı.",
					Css = "danger"
				});
				return View();
			}

			var model = new AccountModel()
			{
				FullName = user.FullName,
				UserName = user.UserName,
				Email = user.Email
			};

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Manage(AccountModel model)
		{

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
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				TempData["message"] = new ResultModels()
				{
					Title = "Bağlantı Hatası",
					Message = "Kullanıcı bilgileri bulunamadı, lütfen tekrar deneyin.",
					Css = "danger"
				};
				return RedirectToAction("Login", "Account");
			}




			user.FullName = model.FullName;
			user.UserName = model.UserName;
			user.Email = model.Email;

			if (model.Email != user.Email)
			{
				var code = await _userManager.GeneratePasswordResetTokenAsync(user);
				var callbackUrl = Url.Action("ResetPassword", "Account", new
				{
					userId = user.Id,
					token = code
				});
				string siteUrl = "https://localhost:7076";
				string resetUrl = $"{siteUrl}{callbackUrl}";

				string body = $"Şifrenizi yenilemek için linke <a href='{resetUrl}'> tıklayınız.</a>";

				MailHelper.SendEmail(body, model.Email, "ETRADE Şifre Sıfırlama");
				TempData.Put("message", new ResultModels()
				{
					Title = "Şifre Sıfırlama",
					Message = "Şifre sıfırlama linki email adresinize gönderilmiştir.",
					Css = "success"
				});
				return RedirectToAction("Login");
			}

			var result = await _userManager.UpdateAsync(user);


			if (result.Succeeded)
			{
				TempData.Put("message", new ResultModels()
				{
					Title = "Hesap Bilgileri Güncellendi",
					Message = "Bilgileriniz başarıyla güncellenmiştir.",
					Css = "success"
				});
				return RedirectToAction("Index", "Home");
			}

			TempData.Put("message", new ResultModels()
			{
				Title = "Hata",
				Message = "Bilgileriniz güncellenemedi. Lütfen tekrar deneyin.",
				Css = "danger"
			});
			return View(model);
		}


		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ForgotPassword(string email)
		{
			if (string.IsNullOrEmpty(email))
			{
				TempData.Put("message", new ResultModels()
				{
					Title = "Şifremi Unuttum",
					Message = "Lütfen Email adresini boş bırakmayınız",
					Css = "danger"
				});
				return View();
			}

			var user = await _userManager.FindByEmailAsync(email);

			if (user is null)
			{
				TempData.Put("message", new ResultModels()
				{
					Title = "Şifremi Unuttum",
					Message = "Bu Email adresiyle bir kullanıcı bulunamadı",
					Css = "danger"
				});
				return View();
			}

			var code = await _userManager.GeneratePasswordResetTokenAsync(user);
			var callbackUrl = Url.Action("ResetPassword", "Account", new { token = code, userId = user.Id });

			string siteUrl = "https://localhost:7136";
			string activeUrl = $"{siteUrl}{callbackUrl}";

			string body = $@"
			<p>Merhaba {user.UserName},</p>
			<p>Şifrenizi yenilemek için aşağıdaki bağlantıya tıklayın:</p>
			<a href='{activeUrl}'>Şifremi Yenile</a>";

			MailHelper.SendEmail(body, user.Email, "Prodora Şifre Yenileme");

			TempData.Put("message", new ResultModels()
			{
				Title = "Şifre Sıfırlama",
				Message = "Şifre yenileme bağlantısı e-posta adresinize gönderildi.",
				Css = "success"
			});

			return RedirectToAction("Login", "Account");
		}


		public IActionResult ResetPassword(string token)
		{
			if (token == null)
			{
				return RedirectToAction("Home", "Index");
			}

			var model = new ResetPasswordModel { Token = token };

			return View(model);
		}


		[HttpPost]
		public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
		{

			if (ModelState.IsValid!)
			{
				return View(model);
			}

			var user = await _userManager.FindByEmailAsync(model.Email);

			if (user is null)
			{
				TempData.Put("message", new ResultModels()
				{
					Title = "Şifre Sıfırlama",
					Message = "Bu email adresiyle kayıtlı kullanıcı bulunamadı.",
					Css = "danger"
				});
			}

			var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

			if (result.Succeeded)
			{
				return RedirectToAction("Login");
			}
			else
			{
				TempData.Put("message", new ResultModels()
				{
					Css = "danger",
					Title = "Şifre Sıfırlama",
					Message = "Şifre sıfırlama işlemi başarısız oldu. Lütfen tekrar deneyin."
				});

			}

			return View(model);
		}

	}
}
