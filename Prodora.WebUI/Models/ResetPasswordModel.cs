using System.ComponentModel.DataAnnotations;

namespace Prodora.WebUI.Models
{
	public class ResetPasswordModel
	{
		public string Token { get; set; }
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
