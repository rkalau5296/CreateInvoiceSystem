using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Frontend.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Podaj adres e-mail")]
        [EmailAddress(ErrorMessage = "Niepoprawny format adresu")]
        public string Email { get; set; } = string.Empty;
    }
}
