using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Prodora.WebUI.EmailServices;

/// <summary>Creates account links without trusting an arbitrary Host header.</summary>
public sealed class AccountLinkBuilder(IConfiguration configuration, IHostEnvironment environment)
{
    public string Create(IUrlHelper urls, HttpRequest request, string action, object values)
    {
        var path = urls.Action(action, "Account", values)
            ?? throw new InvalidOperationException("Account link route is unavailable.");
        var configured = configuration["Site:PublicBaseUrl"];
        Uri origin;
        if (!string.IsNullOrWhiteSpace(configured))
        {
            if (!TryParseOrigin(configured, out origin) ||
                (!environment.IsDevelopment() && origin.Scheme != Uri.UriSchemeHttps))
                throw new InvalidOperationException("Site:PublicBaseUrl must be an absolute HTTPS origin outside Development.");
        }
        else
        {
            if (!environment.IsDevelopment() || !IsLoopbackHost(request.Host.Host) ||
                request.Scheme is not ("http" or "https"))
                throw new InvalidOperationException("A trusted Site:PublicBaseUrl is required for account email links.");
            origin = new Uri($"{request.Scheme}://{request.Host}");
        }
        return new Uri(origin, path).AbsoluteUri;
    }

    public static bool TryParseOrigin(string? value, out Uri origin)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var parsed) &&
            parsed.Scheme is "http" or "https" &&
            parsed.AbsolutePath == "/" && string.IsNullOrEmpty(parsed.Query) &&
            string.IsNullOrEmpty(parsed.Fragment) && string.IsNullOrEmpty(parsed.UserInfo))
        {
            origin = parsed;
            return true;
        }
        origin = null!;
        return false;
    }

    private static bool IsLoopbackHost(string host) =>
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
        (IPAddress.TryParse(host, out var ip) && IPAddress.IsLoopback(ip));
}
