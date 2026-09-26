using System.ComponentModel.DataAnnotations;
namespace Prodora.WebUI.Models;
public class AccountModel
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, StringLength(256)] public string UserName { get; set; } = "";
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = "";
    [DataType(DataType.Password)] public string? CurrentPassword { get; set; }
}
public class EmailRequestModel
{
    [Required(ErrorMessage = "E-posta adresini gir."), EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi gir."), StringLength(256)]
    public string Email { get; set; } = "";
}
