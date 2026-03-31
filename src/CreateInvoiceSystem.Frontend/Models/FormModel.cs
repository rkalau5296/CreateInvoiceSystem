using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Frontend.Models
{
    public class FormModel
    {
        [Required(ErrorMessage = "Podaj stare hasło")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Podaj nowe hasło")]
        [MinLength(6, ErrorMessage = "Hasło musi mieć min. 6 znaków")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{6,}$",
        ErrorMessage = "Hasło musi zawierać małe i duże litery, cyfrę oraz znak specjalny.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Powtórz nowe hasło")]
        [Compare(nameof(NewPassword), ErrorMessage = "Hasła nie są identyczne!")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
