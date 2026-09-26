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
		[EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi gir.")]
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
