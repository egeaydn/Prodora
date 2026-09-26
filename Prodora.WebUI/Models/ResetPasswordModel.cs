using System.ComponentModel.DataAnnotations;

namespace Prodora.WebUI.Models;

public class ResetPasswordModel
{
    [Required(ErrorMessage = "Şifre yenileme bağlantısı eksik. Yeni bir bağlantı iste.")]
    public string Token { get; set; } = "";

    [Required(ErrorMessage = "E-posta adresini gir.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi gir.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Yeni şifreni gir.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}
