using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prodora.Business.Abstract;
using Prodora.WebUI.EmailServices;
using Prodora.WebUI.Filters;
using Prodora.WebUI.Extensions;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers;
public class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
    IBasketServices baskets, IAccountEmailSender emailSender, AccountLinkBuilder links) : Controller
{
    [HttpGet] public IActionResult Register() => View();
    [HttpPost, EnableRateLimiting("account-email")]
    public async Task<IActionResult> Register(RegisterPage model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.UserName, Email = model.Email.Trim(), FullName = model.FullName };
        var result = await users.CreateAsync(user, model.Password);
        if (!result.Succeeded) { AddErrors(result); return View(model); }
        var sent = await SendConfirmation(user);
        Notice("Hesabın oluşturuldu", sent ? "Hesabını etkinleştirmek için e-postandaki bağlantıyı kullan."
            : "Onay e-postası gönderilemedi. Giriş sayfasından yeni bir onay bağlantısı isteyebilirsin.", sent ? "success" : "warning");
        return RedirectToAction(nameof(Login));
    }
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string? token, string? userId)
    {
        var user = string.IsNullOrWhiteSpace(userId) ? null : await users.FindByIdAsync(userId);
        if (user == null || string.IsNullOrWhiteSpace(token) ||
            (!user.EmailConfirmed && !(await users.ConfirmEmailAsync(user, token)).Succeeded))
        {
            Notice("Bağlantı kullanılamıyor", "Yeni bir hesap onay bağlantısı iste.", "warning");
            return RedirectToAction(nameof(ResendConfirmation));
        }
        baskets.InitialBasket(user.Id);
        Notice("Hesabın hazır", "E-posta adresin doğrulandı. Giriş yapabilirsin.");
        return RedirectToAction(nameof(Login));
    }
    [HttpGet] public IActionResult ResendConfirmation() => View(new EmailRequestModel());
    [HttpPost, EnableRateLimiting("account-email")]
    public async Task<IActionResult> ResendConfirmation(EmailRequestModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await users.FindByEmailAsync(model.Email.Trim());
        if (user != null && !user.EmailConfirmed && !await SendConfirmation(user))
        {
            ModelState.AddModelError("", "E-posta şu anda gönderilemedi. Biraz sonra tekrar dene.");
            return View(model);
        }
        Notice("E-postanı kontrol et", "Adresin doğrulanmayı bekleyen bir hesaba aitse onay bağlantısı gönderildi.");
        return RedirectToAction(nameof(Login));
    }
    private async Task<bool> SendConfirmation(ApplicationUser user)
    {
        if (string.IsNullOrEmpty(user.Email)) return false;
        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        var url = links.Create(Url, Request, nameof(ConfirmEmail), new { userId = user.Id, token });
        return await emailSender.SendAsync(EmailTemplates.ConfirmAccount(user.FullName, url), user.Email);
    }
    [HttpGet] public IActionResult Login(string? returnUrl = null) => View(new LoginModel { ReturnUrl = returnUrl });
    [HttpPost, EnableRateLimiting("account-login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        ModelState.Remove(nameof(model.ReturnUrl));
        if (!ModelState.IsValid) return View(model);
        var user = await users.FindByEmailAsync(model.Email.Trim());
        if (user != null)
        {
            var result = await signIn.PasswordSignInAsync(user, model.Password, true, true);
            if (result.Succeeded) return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "~/");
            if (result.IsLockedOut) ModelState.AddModelError("", "Hesabın geçici olarak kilitlendi. Birkaç dakika sonra tekrar dene.");
            else if (result.IsNotAllowed) ModelState.AddModelError("", "Önce e-posta adresini doğrula. Aşağıdaki bağlantıdan yeni onay e-postası isteyebilirsin.");
            else ModelState.AddModelError("", "E-posta veya şifre hatalı.");
        }
        else ModelState.AddModelError("", "E-posta veya şifre hatalı.");
        return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
    public IActionResult AccessDenied() => View();
    [Authorize, HttpGet]
    public async Task<IActionResult> Manage()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        return View(new AccountModel { FullName = user.FullName, UserName = user.UserName!, Email = user.Email! });
    }
    [Authorize, HttpPost, EnableRateLimiting("account-email")]
    public async Task<IActionResult> Manage(AccountModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        var newEmail = model.Email.Trim();
        var emailChanged = users.NormalizeEmail(user.Email) != users.NormalizeEmail(newEmail);
        if (emailChanged)
        {
            if (string.IsNullOrEmpty(model.CurrentPassword) || !await users.CheckPasswordAsync(user, model.CurrentPassword))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "E-posta adresini değiştirmek için mevcut şifreni doğru gir.");
                return View(model);
            }
            var existing = await users.FindByEmailAsync(newEmail);
            if (existing != null && existing.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Bu e-posta adresi kullanılıyor.");
                return View(model);
            }
        }
        user.FullName = model.FullName;
        user.UserName = model.UserName;
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded) { AddErrors(result); return View(model); }
        await signIn.RefreshSignInAsync(user);
        if (emailChanged)
        {
            var token = await users.GenerateChangeEmailTokenAsync(user, newEmail);
            var url = links.Create(Url, Request, nameof(ConfirmEmailChange), new { userId = user.Id, email = newEmail, token });
            var sent = await emailSender.SendAsync(EmailTemplates.ChangeEmail(user.FullName, url), newEmail);
            Notice("Hesap bilgileri kaydedildi", sent
                ? "Yeni e-posta adresine onay bağlantısı gönderildi. Onaylayana kadar mevcut adresin geçerli."
                : "E-posta değişikliği gönderilemedi; mevcut adresin geçerli. Biraz sonra tekrar dene.", sent ? "success" : "warning");
        }
        else Notice("Bilgiler kaydedildi", "Hesap bilgilerin güncellendi.");
        return RedirectToAction(nameof(Manage));
    }
    [HttpGet]
    public async Task<IActionResult> ConfirmEmailChange(string? userId, string? email, string? token)
    {
        var user = string.IsNullOrWhiteSpace(userId) ? null : await users.FindByIdAsync(userId);
        if (user == null || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token) ||
            !(await users.ChangeEmailAsync(user, email, token)).Succeeded)
            Notice("Bağlantı kullanılamıyor", "Bağlantının süresi dolmuş, kullanılmış veya adres başka bir hesapta kullanılıyor olabilir. Hesabından yeniden değişiklik iste.", "warning");
        else
        {
            if (users.GetUserId(User) == user.Id) await signIn.RefreshSignInAsync(user);
            Notice("E-posta adresin güncellendi", "Yeni adresinle giriş yapabilirsin.");
        }
        return RedirectToAction(nameof(Login));
    }
    [HttpGet] public IActionResult ForgotPassword() => View();
    [HttpPost, EnableRateLimiting("account-email")]
    public async Task<IActionResult> ForgotPassword(EmailRequestModel model)
    {
        if (!ModelState.IsValid) return View();
        var user = await users.FindByEmailAsync(model.Email.Trim());
        if (user != null && user.EmailConfirmed)
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            var url = links.Create(Url, Request, nameof(ResetPassword), new { token });
            if (!await emailSender.SendAsync(EmailTemplates.ResetPassword(user.FullName, url), user.Email!))
            {
                ModelState.AddModelError("", "E-posta şu anda gönderilemedi. Biraz sonra tekrar dene.");
                return View();
            }
        }
        Notice("E-postanı kontrol et", "Adresin doğrulanmış bir hesaba aitse şifre yenileme bağlantısı gönderildi.");
        return RedirectToAction(nameof(Login));
    }
    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult ResetPassword(string? token)
        => string.IsNullOrWhiteSpace(token) ? RedirectToAction(nameof(ForgotPassword)) : View(new ResetPasswordModel { Token = token });
    [HttpPost, EnableRateLimiting("account-reset"), ResetPasswordFormRecovery, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await users.FindByEmailAsync(model.Email.Trim());
        var result = user == null ? IdentityResult.Failed(new IdentityError { Code = "InvalidToken" })
            : await users.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            Notice("Şifren güncellendi", "Yeni şifrenle giriş yapabilirsin.");
            return RedirectToAction(nameof(Login));
        }
        if (result.Errors.Any(e => e.Code == "InvalidToken"))
            ModelState.AddModelError("", "Şifre yenileme bağlantısı geçersiz, süresi dolmuş veya daha önce kullanılmış. Yeni bir bağlantı iste.");
        else AddErrors(result);
        return View(model);
    }
    private void Notice(string title, string message, string css = "success")
        => TempData.Put("message", new ResultModels { Title = title, Message = message, Css = css });
    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Code switch
            {
                "DuplicateEmail" => "Bu e-posta adresi kullanılıyor.",
                "DuplicateUserName" => "Bu kullanıcı adı kullanılıyor.",
                "InvalidEmail" => "Geçerli bir e-posta adresi gir.",
                "InvalidUserName" => "Geçerli bir kullanıcı adı gir.",
                _ when error.Code.StartsWith("Password") => "Şifren en az 6 karakter; büyük harf, küçük harf, rakam ve özel karakter içermeli.",
                _ => "Bilgiler kaydedilemedi. Kontrol edip tekrar dene."
            });
    }
}
