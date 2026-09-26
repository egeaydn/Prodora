using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;

namespace Prodora.WebUI.EmailServices;

public interface IAccountEmailSender
{
    Task<bool> SendAsync(BrandedEmail email, string recipient);
}

public sealed class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string From { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public bool EnableSsl { get; set; } = true;
}

public sealed class SmtpEmailSender(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSender> logger) : IAccountEmailSender
{
    public async Task<bool> SendAsync(BrandedEmail email, string recipient)
    {
        var config = settings.Value;
        if (string.IsNullOrWhiteSpace(config.From) || string.IsNullOrWhiteSpace(config.Password))
        {
            logger.LogWarning("SMTP settings are incomplete.");
            return false;
        }
        try
        {
            using var message = new MailMessage {
                From = new MailAddress(config.From, "Prodora"),
                Subject = email.Subject, SubjectEncoding = Encoding.UTF8,
                Body = email.TextBody, BodyEncoding = Encoding.UTF8, IsBodyHtml = false
            };
            message.To.Add(recipient);
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(email.HtmlBody, Encoding.UTF8, "text/html"));
            using var smtp = new SmtpClient(config.Host, config.Port) {
                EnableSsl = config.EnableSsl, UseDefaultCredentials = false,
                Credentials = new NetworkCredential(config.UserName, config.Password)
            };
            await smtp.SendMailAsync(message);
            return true;
        }
        catch (Exception error) when (error is SmtpException or FormatException or InvalidOperationException)
        {
            // Do not put email tokens, credentials or recipient details in logs.
            logger.LogWarning("Email delivery failed ({FailureType}).", error.GetType().Name);
            return false;
        }
    }
}
