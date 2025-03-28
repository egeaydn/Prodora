using Prodora.WebUI.Identity;

namespace Prodora.WebUI.Models
{
	public class AccountModel :ApplicationUser
	{
		public string FullName { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
	}
}
