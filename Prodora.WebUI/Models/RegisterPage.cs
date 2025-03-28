using System.ComponentModel.DataAnnotations;

namespace Prodora.WebUI.Models
{
	public class RegisterPage
	{
		[Required]
		public string FullName { get; set; }
		[Required]
		public string UserName { get; set; }
		[Required]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		[Required]
		[DataType(DataType.Password)]
		[Compare("Password")]
		public string RePassword { get; set; }
	}
}
