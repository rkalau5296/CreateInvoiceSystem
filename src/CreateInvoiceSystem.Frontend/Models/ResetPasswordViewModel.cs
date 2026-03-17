using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Frontend.Models;

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "Hasło jest wymagane")]
    [MinLength(6, ErrorMessage = "Minimum 6 znaków")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Powtórz hasło")]
    [Compare("Password", ErrorMessage = "Hasła muszą być identyczne")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
