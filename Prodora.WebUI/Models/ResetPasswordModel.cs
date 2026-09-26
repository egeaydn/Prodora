using System.ComponentModel.DataAnnotations;
namespace Prodora.WebUI.Models;
public class ResetPasswordModel
{
    [Required(ErrorMessage = "Yeni bir şifre yenileme bağlantısı iste.")] public string Token { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
}
