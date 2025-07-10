
using System.Net;
using System.Net.Mail;

namespace Prodora.WebUI.EmailServices
{
	public static class MailHelper
	{
		// Tek bir e-posta adresine mail göndermek için kullanılan metot.
		// Varsayılan olarak HTML formatında e-posta gönderir.
		public static bool SendEmail(string body, string to, string subject, bool isHtml = true)
		{
			// Tek bir adresi, birden fazla alıcıya mail gönderen metota liste olarak iletiyor.
			return SendEmail(body, new List<string> { to }, subject, isHtml);
		}

		// Birden fazla alıcıya mail göndermeyi sağlayan asıl metot.
		private static bool SendEmail(string body, List<string> to, string subject, bool isHtml)
		{
			bool result = false; // Mail gönderim sonucunu tutan değişken.

			try
			{
				var message = new MailMessage(); // Yeni bir e-posta mesajı nesnesi oluşturuluyor.

				// Gönderen e-posta adresi belirleniyor.
				message.From = new MailAddress("prodoramailservices@gmail.com");

				// Alıcı adresler mesajın "To" (Kime) alanına ekleniyor.
				to.ForEach(x =>
				{
					message.To.Add(new MailAddress(x));
				});

				message.Subject = subject;  // Mailin konusu belirleniyor.
				message.Body = body;        // Mailin içeriği belirleniyor.
				message.IsBodyHtml = isHtml; // Gövdenin HTML olup olmadığı belirleniyor.

				// SMTP istemcisi oluşturuluyor ve Gmail'in SMTP sunucusu kullanılıyor.
				using (var smtp = new SmtpClient("smtp.gmail.com", 587))
				{
					smtp.EnableSsl = true; // Güvenli bağlantı (SSL/TLS) etkinleştiriliyor.

					// SMTP kimlik doğrulaması için kullanıcı adı ve şifre belirleniyor.
					smtp.Credentials = new NetworkCredential("prodoramailservices@gmail.com", "yiej zsri rjqi rwls");


					smtp.UseDefaultCredentials = false; // Varsayılan kimlik bilgileri kullanılmıyor.

					smtp.Send(message); // E-posta gönderme işlemi gerçekleşiyor.
					result = true; // Mail başarıyla gönderildiği için sonuç true yapılıyor.
				}
			}
			catch (Exception e)
			{
				// Eğer bir hata oluşursa, hatayı konsola yazdırıp sonucu false olarak döndürüyoruz.
				Console.WriteLine(e);
				result = false;
			}

			return result; // Mail gönderme işleminin sonucunu döndür.

		}

		// Modern ve dinamik HTML e-posta şablonu oluşturan fonksiyon
		private static string GetModernEmailTemplate(string title, string message, string buttonText = null, string buttonUrl = null)
		{
			return $@"
			<html>
			<head>
			  <meta charset='UTF-8'>
			  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
			  <title>{title}</title>
			</head>
			<body style='background:#f4f4f7;margin:0;padding:0;'>
			  <table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f4f7;padding:40px 0;'>
				<tr>
				  <td align='center'>
					<table width='100%' style='max-width:600px;background:#fff;border-radius:10px;box-shadow:0 2px 8px rgba(0,0,0,0.07);padding:40px;'>
					  <tr>
						<td align='center' style='padding-bottom:24px;'>
						  <h1 style='font-family:sans-serif;color:#222;font-size:28px;margin:0;'>{title}</h1>
						</td>
					  </tr>
					  <tr>
						<td style='font-family:sans-serif;color:#444;font-size:16px;line-height:1.6;padding-bottom:32px;'>
						  {message}
						</td>
					  </tr>
					  {(string.IsNullOrEmpty(buttonText) || string.IsNullOrEmpty(buttonUrl) ? "" : $@"<tr><td align='center' style='padding-bottom:32px;'><a href='{buttonUrl}' style='display:inline-block;padding:12px 32px;background:#4f8cff;color:#fff;border-radius:6px;text-decoration:none;font-weight:bold;font-size:16px;'>{buttonText}</a></td></tr>")}
					  <tr>
						<td align='center' style='font-family:sans-serif;color:#aaa;font-size:13px;padding-top:24px;'>
						  Bu e-posta Prodora tarafından gönderilmiştir.<br/>© {DateTime.Now.Year} Prodora
						</td>
					  </tr>
					</table>
				  </td>
				</tr>
			  </table>
			</body>
			</html>";
		}

		// Modern şablon ile mail gönderen overload
		public static bool SendModernEmail(string to, string title, string message, string buttonText = null, string buttonUrl = null)
		{
			string htmlBody = GetModernEmailTemplate(title, message, buttonText, buttonUrl);
			return SendEmail(htmlBody, to, title, true);
		}
	}
}
