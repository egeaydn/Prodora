using Prodora.WebUI.EmailServices;

namespace Prodora.WebUI.Seo;

public static class SiteUrl
{
    /// <summary>
    /// Absolute URL for sitemaps and canonical links. The configured public origin wins, because a request's Host header
    /// can be forged; the request origin is only a fallback for local development.
    /// </summary>
    public static string Absolute(HttpContext context, string path)
    {
        var configured = context.RequestServices.GetService<IConfiguration>()?["Site:PublicBaseUrl"];
        var origin = AccountLinkBuilder.TryParseOrigin(configured, out var parsed)
            ? parsed.GetLeftPart(UriPartial.Authority)
            : $"{context.Request.Scheme}://{context.Request.Host}";
        return origin.TrimEnd('/') + (path.StartsWith('/') ? path : "/" + path);
    }
}
