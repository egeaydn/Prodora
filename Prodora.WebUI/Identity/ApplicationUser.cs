using Microsoft.AspNetCore.Identity;

namespace Prodora.WebUI.Identity
{
	public class ApplicationUser : IdentityUser
	{
		public string FullName { get; set; }
	}
}
