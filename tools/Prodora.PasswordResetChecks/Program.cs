using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Business.Concrate;
using Prodora.WebUI.Identity;
using Prodora.WebUI.EmailServices;
using Microsoft.AspNetCore.RateLimiting;

// Real MVC views, antiforgery and Identity token/password providers; no SQL or SMTP.
// The HTTP client models which cookies a browser sends on a top-level email navigation.
var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Prodora.WebUI"));
var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
    ApplicationName = typeof(Prodora.WebUI.Controllers.AccountController).Assembly.FullName,
    ContentRootPath = root, WebRootPath = Path.Combine(root, "wwwroot"), EnvironmentName = "Development"
});
builder.Logging.ClearProviders();
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddSingleton<IUserStore<ApplicationUser>, MemoryUserStore>();
builder.Services.AddIdentityCore<ApplicationUser>().AddSignInManager().AddDefaultTokenProviders();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, ApplicationCookieSettings.Configure);
builder.Services.AddAuthorization();
builder.Services.AddScoped<IBasketServices>(_ => new BasketManager(null!));
builder.Services.AddSingleton<NoopEmailSender>();
builder.Services.AddSingleton<IAccountEmailSender>(sp => sp.GetRequiredService<NoopEmailSender>());
builder.Services.AddScoped<AccountLinkBuilder>();
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("account-login", limit =>
{
    limit.PermitLimit = 100;
    limit.Window = TimeSpan.FromMinutes(1);
}).AddFixedWindowLimiter("account-reset", limit =>
{
    limit.PermitLimit = 100;
    limit.Window = TimeSpan.FromMinutes(1);
}).AddFixedWindowLimiter("account-email", limit =>
{
    limit.PermitLimit = 100;
    limit.Window = TimeSpan.FromMinutes(1);
}));
builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
var app = builder.Build();
app.Urls.Add("http://127.0.0.1:0");
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapGet("/__checks/signin", async (HttpContext context) => {
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "reset-check"), new Claim(ClaimTypes.Name, "reset-check") };
    await context.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme)));
    return Results.Ok();
});
try
{
    await app.StartAsync();
    using var scope = app.Services.CreateScope();
    var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var user = new ApplicationUser { Id = "reset-check", UserName = "reset-check", Email = "reset@example.invalid", FullName = "Reset Test", EmailConfirmed = true };
    var created = await manager.CreateAsync(user, "Initial!123");
    Ensure(created.Succeeded, "memory test user");
    using var http = new HttpClient(new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false }) { BaseAddress = new Uri(app.Urls.Single()) };
    var sender = scope.ServiceProvider.GetRequiredService<NoopEmailSender>();
    var forgotForm = await http.GetAsync("/Account/ForgotPassword");
    var forgotHtml = await forgotForm.Content.ReadAsStringAsync();
    var forgotCsrf = WebUtility.HtmlDecode(Regex.Match(forgotHtml, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    var forgotCookie = forgotForm.Headers.GetValues("Set-Cookie").Single(h => h.StartsWith(".AspNetCore.Antiforgery.")).Split(';')[0];
    Ensure(forgotCsrf.Length > 0, "forgot-password form issues antiforgery token");
    async Task<HttpResponseMessage> RequestResetWithHost(string host)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/Account/ForgotPassword");
        request.Headers.Host = host;
        request.Headers.Add("Cookie", forgotCookie);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
            ["Email"] = user.Email!, ["__RequestVerificationToken"] = forgotCsrf
        });
        return await http.SendAsync(request);
    }
    var forgedHost = await RequestResetWithHost("attacker.example");
    Ensure((int)forgedHost.StatusCode >= 400 && sender.Count == 0, "forged Host does not enter password-reset email");
    var localReset = await RequestResetWithHost(http.BaseAddress!.Authority);
    Ensure(localReset.StatusCode == HttpStatusCode.Redirect && sender.Count == 1 &&
        new Uri(Regex.Match(sender.Last!.TextBody, @"https?://[^\s]+").Value).Authority == http.BaseAddress.Authority,
        "trusted loopback host produces a local password-reset link");
    Console.WriteLine("PASS account email links reject forged hosts");
    var signIn = await http.GetAsync("/__checks/signin");
    var authHeader = signIn.Headers.GetValues("Set-Cookie").Single(h => h.StartsWith("PRODORA.Security.Cookie="));
    var authCookie = authHeader.Split(';')[0];
    var isStrict = authHeader.Contains("samesite=strict", StringComparison.OrdinalIgnoreCase);
    var resetToken = await manager.GeneratePasswordResetTokenAsync(user);
    var fromEmail = await GetForm(resetToken, isStrict ? null : authCookie);
    var post = await Submit(fromEmail, resetToken, "Replacement!123", authCookie);
    Console.WriteLine($"Email navigation with existing session: cookie={(isStrict ? "Strict" : "Lax")}; POST={(int)post.StatusCode}");
    Ensure(post.StatusCode == HttpStatusCode.Redirect, "email-to-reset POST should reach the action, not return 400");
    Ensure(await manager.CheckPasswordAsync(user, "Replacement!123"), "password was changed");
    Console.WriteLine("PASS signed-in email navigation and real password reset");

    var newToken = await manager.GeneratePasswordResetTokenAsync(user);
    var anonymous = await GetForm(newToken, null);
    var weak = await Submit(anonymous, newToken, "abc", null);
    Ensure(weak.StatusCode == HttpStatusCode.OK, "weak password displays the form");
    Ensure(await manager.CheckPasswordAsync(user, "Replacement!123"), "weak password did not alter account");
    var strong = await Submit(anonymous, newToken, "Anonymous!456", null);
    Ensure(strong.StatusCode == HttpStatusCode.Redirect && await manager.CheckPasswordAsync(user, "Anonymous!456"), "anonymous reset");
    Console.WriteLine("PASS anonymous reset and weak-password rejection");

    var invalid = await GetForm(newToken, null);
    var reused = await Submit(invalid, newToken, "Unexpected!789", null);
    Ensure(reused.StatusCode == HttpStatusCode.OK && await manager.CheckPasswordAsync(user, "Anonymous!456"), "used reset token rejected");
    Console.WriteLine("PASS used reset token cannot change the password");

    // Model a form opened before sign-in, or an old Strict cookie left in a browser.
    var staleResetToken = await manager.GeneratePasswordResetTokenAsync(user);
    var staleForm = await GetForm(staleResetToken, null);
    var rejected = await Submit(staleForm, staleResetToken, "Retry!12345", authCookie);
    var recoveryHtml = await rejected.Content.ReadAsStringAsync();
    Ensure(rejected.StatusCode == HttpStatusCode.BadRequest && recoveryHtml.Contains("validation-summary-errors"), "stale form gets a recovery view");
    Ensure(!recoveryHtml.Contains("Retry!12345") && await manager.CheckPasswordAsync(user, "Anonymous!456"), "rejected password is neither applied nor echoed");
    var refreshedCsrf = WebUtility.HtmlDecode(Regex.Match(recoveryHtml, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    Ensure(refreshedCsrf.Length > 0, "recovery issues a fresh form token");
    var refreshedCookie = rejected.Headers.TryGetValues("Set-Cookie", out var recoverySetCookies)
        ? recoverySetCookies.FirstOrDefault(h => h.StartsWith(".AspNetCore.Antiforgery."))?.Split(';')[0] ?? staleForm.Cookie
        : staleForm.Cookie;
    Ensure(refreshedCookie.Length > 0, "recovery retains or issues an antiforgery cookie");
    var retry = await Submit((refreshedCsrf, refreshedCookie), staleResetToken, "Retry!12345", authCookie);
    Ensure(retry.StatusCode == HttpStatusCode.Redirect && await manager.CheckPasswordAsync(user, "Retry!12345"), "retry after session change succeeds");
    Console.WriteLine("PASS stale-session recovery, no password echo and successful retry");

    var withoutCsrf = await http.PostAsync("/Account/ResetPassword", new FormUrlEncodedContent(new Dictionary<string,string> {
        ["Token"] = newToken, ["Email"] = user.Email!, ["Password"] = "Unexpected!789"
    }));
    Ensure(withoutCsrf.StatusCode == HttpStatusCode.BadRequest, "CSRF must remain enabled");
    Ensure(await manager.CheckPasswordAsync(user, "Retry!12345"), "CSRF rejection did not change password");
    Console.WriteLine("PASS missing antiforgery token rejected with 400");

    var incompleteToken = await manager.GeneratePasswordResetTokenAsync(user);
    var incompleteForm = await GetForm(incompleteToken, null);
    var missingToken = await Submit(incompleteForm, "", "Retry!12345", null);
    Ensure(missingToken.StatusCode == HttpStatusCode.OK && (await missingToken.Content.ReadAsStringAsync()).Contains("validation-summary-errors"), "missing reset token is explained in the form");
    var noTokenGet = await http.GetAsync("/Account/ResetPassword");
    Ensure(noTokenGet.StatusCode == HttpStatusCode.Redirect && noTokenGet.Headers.Location!.ToString().Contains("ForgotPassword"), "missing link directs to a new link request");
    Console.WriteLine("PASS missing reset-link validation and redirect");

    async Task<(string Csrf, string Cookie)> GetForm(string token, string? auth)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/Account/ResetPassword?token=" + Uri.EscapeDataString(token));
        if (auth != null) request.Headers.Add("Cookie", auth);
        var response = await http.SendAsync(request);
        Ensure(response.StatusCode == HttpStatusCode.OK, "reset form GET");
        Ensure(response.Headers.CacheControl?.NoStore == true, "reset form must not be cached");
        var html = await response.Content.ReadAsStringAsync();
        var csrf = WebUtility.HtmlDecode(Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        var cookie = response.Headers.GetValues("Set-Cookie").Single(h => h.StartsWith(".AspNetCore.Antiforgery.")).Split(';')[0];
        Ensure(csrf.Length > 0, "form contains antiforgery token");
        return (csrf, cookie);
    }
    async Task<HttpResponseMessage> Submit((string Csrf, string Cookie) form, string token, string password, string? auth)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/Account/ResetPassword");
        request.Headers.Add("Cookie", form.Cookie + (auth == null ? "" : "; " + auth));
        request.Content = new FormUrlEncodedContent(new Dictionary<string,string> {
            ["Token"] = token, ["Email"] = user.Email!, ["Password"] = password, ["__RequestVerificationToken"] = form.Csrf
        });
        return await http.SendAsync(request);
    }
}
finally { await app.StopAsync(); await app.DisposeAsync(); }
static void Ensure(bool condition, string description) { if (!condition) throw new InvalidOperationException("FAIL: " + description); }
