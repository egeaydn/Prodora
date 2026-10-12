namespace Prodora.WebUI.Middlewares;

public static class SecurityHeadersExtensions
{
    // Views carry no inline script/style and load nothing from third-party hosts,
    // so the policy can stay strict. Payments run server-side against the iyzico API.
    private const string ContentSecurityPolicy =
        "default-src 'self'; img-src 'self' data: https:; script-src 'self'; style-src 'self'; " +
        "font-src 'self'; connect-src 'self'; object-src 'none'; base-uri 'self'; " +
        "form-action 'self'; frame-ancestors 'none'";

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers["Content-Security-Policy"] = ContentSecurityPolicy;
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
                return Task.CompletedTask;
            });
            await next();
        });
}
