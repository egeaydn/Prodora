using System.Net;

namespace Prodora.WebUI.EmailServices;

/// <summary>HTML and plain-text versions of the same transactional message.</summary>
public sealed record BrandedEmail(string Subject, string HtmlBody, string TextBody);

public static class EmailTemplates
{
    public static BrandedEmail ChangeEmail(string? name, string confirmationUrl) => Build(
        "Prodora e-posta adresini onayla", "Yeni e-posta adresini doğrulayarak değişikliği tamamla.",
        "E-POSTA DEĞİŞİKLİĞİ", "Yeni adresini\nonayla.", name,
        "Prodora hesabının e-posta adresini değiştirmek için bir istek aldık. Değişiklik, bu adrese gönderilen bağlantıyı onayladığında tamamlanır.",
        "Yeni adresimi onayla", confirmationUrl, "ESKİ ADRESİN KORUNUYOR",
        "Onay tamamlanana kadar mevcut e-posta adresinle hesabını kullanmaya devam edebilirsin.",
        "Bu isteği sen yapmadıysan bağlantıyı kullanma. E-posta adresin değişmez.", "#eef1e9");
    public static BrandedEmail ConfirmAccount(string? name, string confirmationUrl) => Build(
        "Prodora hesabını onayla",
        "Prodora'ya hoş geldin. Hesabını onaylayarak alışverişe başlayabilirsin.",
        "HESAP ONAYI", "İyi seçimlere\nhoş geldin.", name,
        "Seni burada görmek güzel. Prodora hesabını kullanmaya başlamak için e-posta adresini onaylaman yeterli.",
        "Hesabımı onayla", confirmationUrl,
        "SONRA NE VAR?",
        "Yeni parçaları keşfet, favori seçimlerini sepetine ekle ve siparişlerini tek yerden takip et.",
        "Bu hesabı sen oluşturmadıysan herhangi bir işlem yapmana gerek yok.",
        "#eef1e9");

    public static BrandedEmail ResetPassword(string? name, string resetUrl) => Build(
        "Prodora şifreni yenile",
        "Hesabına yeniden erişmek için yeni bir şifre belirle.",
        "ŞİFRE YENİLEME", "Hesabına yeni\nbir başlangıç.", name,
        "Hesabının şifresini yenilemek için bir istek aldık. Aşağıdaki bağlantıdan yeni şifreni belirleyebilirsin.",
        "Yeni şifre belirle", resetUrl,
        "KÜÇÜK BİR HATIRLATMA",
        "Başka hesaplarında kullanmadığın güçlü bir şifre seç. Bu bağlantıyı ve şifreni kimseyle paylaşma.",
        "Bu isteği sen yapmadıysan e-postayı yok sayabilirsin. Bağlantıyı kullanmadığın sürece şifren değişmez.",
        "#f8f0df");

    // For future notifications; adding a template does not create a new sending trigger.
    public static BrandedEmail Notification(string title, string message, string? buttonText = null, string? buttonUrl = null) => Build(
        title, message, "HESABINDAN BİR HABER", title, null, message,
        buttonText, buttonUrl, "HER ZAMAN YANINDA",
        "Hesabındaki güncel bilgileri ve siparişlerini Prodora üzerinden görüntüleyebilirsin.",
        "Bu e-posta Prodora hesabınla ilgili bilgilendirme amacıyla gönderildi.",
        "#eef1e9");

    private static BrandedEmail Build(string subject, string preview, string label, string heading, string? name,
        string message, string? buttonText, string? buttonUrl, string noteTitle, string note,
        string securityNote, string accent)
    {
        if (subject.Contains('\r') || subject.Contains('\n'))
            throw new ArgumentException("E-posta konusu tek satır olmalıdır.", nameof(subject));

        var hasAction = !string.IsNullOrWhiteSpace(buttonText) && !string.IsNullOrWhiteSpace(buttonUrl);
        if (!hasAction && (buttonText != null || buttonUrl != null))
            throw new ArgumentException("Düğme metni ve bağlantısı birlikte belirtilmelidir.");
        if (hasAction && (!Uri.TryCreate(buttonUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)))
            throw new ArgumentException("E-posta bağlantısı mutlak bir HTTP/HTTPS adresi olmalıdır.", nameof(buttonUrl));

        string H(string? value) => WebUtility.HtmlEncode(value ?? "");
        var greeting = string.IsNullOrWhiteSpace(name) ? "Merhaba," : $"Merhaba {name.Trim()},";
        var action = hasAction ? $"""
            <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="margin:28px 0 0;">
              <tr><td bgcolor="#efb33f" style="border-radius:6px;text-align:center;mso-padding-alt:15px 28px;">
                <a href="{H(buttonUrl)}" style="background:#efb33f;border:1px solid #efb33f;border-radius:6px;color:#191919;display:inline-block;font-size:15px;font-weight:700;line-height:22px;padding:15px 28px;text-decoration:none;mso-padding-alt:0;text-underline-color:#efb33f;"><!--[if mso]><i style="letter-spacing:28px;mso-font-width:-100%;mso-text-raise:22pt;">&nbsp;</i><![endif]--><span style="mso-text-raise:11pt;">{H(buttonText)}</span><!--[if mso]><i style="letter-spacing:28px;mso-font-width:-100%;">&nbsp;</i><![endif]--></a>
              </td></tr>
            </table>
            """ : "";
        var fallback = hasAction ? $"""
            <tr><td class="email-pad" style="padding:0 40px 32px;">
              <p style="margin:0 0 8px;color:#727670;font-size:12px;line-height:20px;">Düğme çalışmıyorsa bu bağlantıyı tarayıcına kopyala:</p>
              <a href="{H(buttonUrl)}" style="color:#4c5850;font-size:12px;line-height:20px;text-decoration:underline;overflow-wrap:anywhere;word-break:break-all;">{H(buttonUrl)}</a>
            </td></tr>
            """ : "";
        const string responsiveStyle = """
              <style>
                body,table,td,a{-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%}
                table,td{mso-table-lspace:0pt;mso-table-rspace:0pt}
                table{border-collapse:collapse}
                @media only screen and (max-width:620px){.email-outer{padding:20px 12px!important}.email-pad{padding-left:24px!important;padding-right:24px!important}.email-heading{font-size:32px!important;line-height:38px!important}}
              </style>
              """;
        var html = $"""
            <!doctype html>
            <html lang="tr">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <meta name="color-scheme" content="light">
              <meta name="supported-color-schemes" content="light">
              <title>{H(subject)}</title>
              {responsiveStyle}
            </head>
            <body style="margin:0;padding:0;width:100%;background:#f4f4ef;color:#202a2d;font-family:'Segoe UI',Arial,sans-serif;">
              <div style="display:none;font-size:1px;line-height:1px;color:#f4f4ef;max-height:0;max-width:0;opacity:0;overflow:hidden;mso-hide:all;">{H(preview)}</div>
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" bgcolor="#f4f4ef">
                <tr><td class="email-outer" align="center" style="padding:40px 16px;">
                  <!--[if mso]><table role="presentation" width="600" cellpadding="0" cellspacing="0" border="0"><tr><td><![endif]-->
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" bgcolor="#ffffff" style="max-width:600px;background:#ffffff;border:1px solid #e1e3dc;">
                    <tr><td class="email-pad" style="padding:30px 40px 26px;border-bottom:1px solid #e9eae5;">
                      <span aria-label=".Prodora" style="font-family:'Segoe UI',Arial,sans-serif;font-size:34px;font-weight:750;letter-spacing:-1.6px;line-height:40px;color:#191919;"><span style="color:#efb33f;">.</span>Prodora</span>
                      <p style="margin:5px 0 0;color:#747a73;font-size:11px;letter-spacing:1.2px;">İYİ SEÇİMLER, HER GÜN.</p>
                    </td></tr>
                    <tr><td class="email-pad" bgcolor="{accent}" style="padding:34px 40px;background:{accent};">
                      <p style="margin:0 0 15px;color:#626b5e;font-size:10px;font-weight:700;letter-spacing:2px;">{H(label)}</p>
                      <h1 class="email-heading" style="margin:0;color:#202a2d;font-size:38px;font-weight:650;letter-spacing:-1.4px;line-height:44px;">{H(heading).Replace("\n", "<br>")}</h1>
                    </td></tr>
                    <tr><td class="email-pad" style="padding:32px 40px;">
                      <p style="margin:0 0 14px;font-size:16px;line-height:26px;font-weight:600;color:#202a2d;">{H(greeting)}</p>
                      <p style="margin:0;font-size:15px;line-height:26px;color:#606963;">{H(message)}</p>
                      {action}
                    </td></tr>
                    <tr><td class="email-pad" style="padding:0 40px 28px;">
                      <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" bgcolor="#f7f7f3">
                        <tr><td style="padding:22px 24px;border-left:3px solid #d8b56a;">
                          <p style="margin:0 0 9px;font-size:10px;font-weight:700;letter-spacing:1.2px;color:#586354;">{H(noteTitle)}</p>
                          <p style="margin:0;font-size:13px;line-height:22px;color:#697166;">{H(note)}</p>
                        </td></tr>
                      </table>
                    </td></tr>
                    <tr><td class="email-pad" style="padding:0 40px 26px;">
                      <p style="margin:0;font-size:12px;line-height:21px;color:#767b74;">{H(securityNote)}</p>
                    </td></tr>
                    {fallback}
                    <tr><td class="email-pad" style="padding:24px 40px;border-top:1px solid #e9eae5;">
                      <p style="margin:0;color:#202a2d;font-size:13px;font-weight:600;">Görüşmek üzere,<br>Prodora</p>
                      <p style="margin:14px 0 0;color:#898e86;font-size:11px;line-height:19px;">Bu otomatik bir hesap bilgilendirmesidir.<br>© {DateTime.Now.Year} Prodora</p>
                    </td></tr>
                  </table>
                  <!--[if mso]></td></tr></table><![endif]-->
                </td></tr>
              </table>
            </body>
            </html>
            """;
        var text = $".Prodora\n{subject}\n\n{greeting}\n\n{message}\n\n"
            + (hasAction ? $"{buttonText}: {buttonUrl}\n\n" : "")
            + $"{noteTitle}\n{note}\n\n{securityNote}\n\nProdora";
        return new BrandedEmail(subject, html, text);
    }
}
