using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Frontend.Models
{
    public class FormModel
    {
        [Required(ErrorMessage = "Podaj stare hasło")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Podaj nowe hasło")]
        [MinLength(6, ErrorMessage = "Hasło musi mieć min. 6 znaków")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Powtórz nowe hasło")]
        [Compare(nameof(NewPassword), ErrorMessage = "Hasła nie są identyczne!")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
