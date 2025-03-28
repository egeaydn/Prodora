using System.ComponentModel.DataAnnotations;

namespace Prodora.WebUI.Models
{
	public class AdminUserModel
	{
		public string FullName { get; set; }
		public string UserName { get; set; }
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }
		public bool IsAdmin { get; set; }
		public bool EmailConfirmed { get; set; }
	}
}
