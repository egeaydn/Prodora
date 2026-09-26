using System.Net;
using System.Text.RegularExpressions;
using Prodora.WebUI.EmailServices;

// This project only renders HTML/text files. It has no SMTP or database dependencies.
var output = args.Length > 0 ? Path.GetFullPath(args[0])
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../artifacts/email-previews"));
Directory.CreateDirectory(output);
File.Copy(Path.Combine(AppContext.BaseDirectory, "index.html"), Path.Combine(output, "index.html"), true);
const string sampleUrl = "http://localhost:5047/Account/ResetPassword?userId=preview&token=preview%2B%2F%3D";
var templates = new Dictionary<string, BrandedEmail>
{
    ["hesap-onayi"] = EmailTemplates.ConfirmAccount("Deniz", sampleUrl.Replace("ResetPassword", "ConfirmEmail")),
    ["eposta-degisikligi"] = EmailTemplates.ChangeEmail("Deniz", sampleUrl.Replace("ResetPassword", "ConfirmEmailChange")),
    ["sifre-yenileme"] = EmailTemplates.ResetPassword("Deniz", sampleUrl),
    ["genel-bildirim"] = EmailTemplates.Notification("Hesabından bir haber.", "Bu, gelecekteki hesap bildirimleri için hazırlanmış örnek içeriktir.", "Hesabıma git", "http://localhost:5047/Account/Manage")
};
foreach (var (name, email) in templates)
{
    File.WriteAllText(Path.Combine(output, name + ".html"), email.HtmlBody);
    File.WriteAllText(Path.Combine(output, name + ".txt"), email.TextBody);
    if (!email.HtmlBody.Contains("lang=\"tr\"") || !email.TextBody.Contains(".Prodora"))
        throw new Exception(name + ": missing brand or language");
    if (email.HtmlBody.Contains("<script", StringComparison.OrdinalIgnoreCase))
        throw new Exception(name + ": scripts must not be used in email");
    Console.WriteLine("PASS " + name);
}
var hostileName = "<img src=x onerror=alert(1)> & \"test\"";
var escaped = EmailTemplates.ResetPassword(hostileName, sampleUrl);
if (escaped.HtmlBody.Contains(hostileName) || !escaped.HtmlBody.Contains(WebUtility.HtmlEncode(hostileName)))
    throw new Exception("Personal data was not HTML-encoded.");
var links = Regex.Matches(escaped.HtmlBody, "href=\"([^\"]*)\"");
if (links.Count != 2 || links.Any(link => WebUtility.HtmlDecode(link.Groups[1].Value) != sampleUrl))
    throw new Exception("Button/fallback links or token encoding changed.");
if (!escaped.TextBody.Contains(sampleUrl))
    throw new Exception("Plain-text version must retain the full link.");
foreach (var invalidUrl in new[] { "javascript:alert(1)", "/relative-link", "data:text/html,test", "" })
{
    try { EmailTemplates.ResetPassword("Deniz", invalidUrl); throw new Exception("Unsafe/missing link accepted."); }
    catch (ArgumentException) { }
}
var noAction = EmailTemplates.Notification("Bilgilendirme", "Düğmesiz bir bildirim.");
if (noAction.HtmlBody.Contains("href=")) throw new Exception("Unexpected notification link.");
Console.WriteLine("PASS HTML escaping, token round-trip, plain text and URL validation");
Console.WriteLine("Preview files: " + output);
