using Microsoft.AspNetCore.Authentication.Cookies;

namespace Prodora.WebUI.Identity;

public static class ApplicationCookieSettings
{
    public static void Configure(CookieAuthenticationOptions options)
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = "PRODORA.Security.Cookie";
        // Email links are top-level cross-site GETs. Preserve the session when the
        // form token is issued, so its identity also matches the subsequent POST.
        options.Cookie.SameSite = SameSiteMode.Lax;
    }
}
