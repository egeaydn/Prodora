
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
		private static bool SendEmail(string body, List<string> to, string subject, bool isHtml, string? textBody = null)
		{
			bool result = false; // Mail gönderim sonucunu tutan değişken.

			try
			{
				using var message = new MailMessage(); // Yeni bir e-posta mesajı nesnesi oluşturuluyor.

				// Gönderen e-posta adresi belirleniyor.
				message.From = new MailAddress("prodoramailservices@gmail.com");

				// Alıcı adresler mesajın "To" (Kime) alanına ekleniyor.
				to.ForEach(x =>
				{
					message.To.Add(new MailAddress(x));
				});

				message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.Subject = subject;  // Mailin konusu belirleniyor.
				message.Body = textBody ?? body;
				message.IsBodyHtml = textBody == null && isHtml;
                if (textBody != null)
                {
                    message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(body, System.Text.Encoding.UTF8, "text/html"));
                } // Gövdenin HTML olup olmadığı belirleniyor.

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

		public static bool SendEmail(BrandedEmail email, string to)
        {
            return SendEmail(email.HtmlBody, new List<string> { to }, email.Subject, true, email.TextBody);
        }

        public static bool SendModernEmail(string to, string title, string message, string? buttonText = null, string? buttonUrl = null)
        {
            return SendEmail(EmailTemplates.Notification(title, message, buttonText, buttonUrl), to);
        }
    }
}
