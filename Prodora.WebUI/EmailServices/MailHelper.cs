
using System.Net;
using System.Net.Mail;

namespace Prodora.WebUI.EmailServices
{
	public static class MailHelper
	{
		public static bool SendEmail(string body, string to, string subject, bool isHtml = true)
		{
			return SendEmail(body, new List<string> { to }, subject, isHtml);
		}

		private static bool SendEmail(string body, List<string> to, string subject, bool isHtml)
		{
			bool result = false;

			try
			{
				var message = new MailMessage();
				message.From = new MailAddress("prodoraemailservice@gmail.com");

				to.ForEach(x =>
				{
					message.To.Add(new MailAddress(x));
				});

				message.Subject = subject;
				message.Body = body;
				message.IsBodyHtml = isHtml;

				using (var smtp = new SmtpClient("smtp.gmail.com",587))
				{
					smtp.EnableSsl = true;
					smtp.Credentials = new NetworkCredential("prodoraemailservice@gmail.com", "VLFUV-KS472-E7Z2U-VEN97-LT922");
					smtp.UseDefaultCredentials = false;
					smtp.Send(message);
					result = true;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				result = false;
			}
			return result;

		}
	}
}
