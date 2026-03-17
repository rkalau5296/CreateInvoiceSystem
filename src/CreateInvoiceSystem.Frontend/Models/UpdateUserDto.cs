using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Frontend.Models
{
    public class UpdateUserDto
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Imię i nazwisko jest wymagane.")]
        [StringLength(100, ErrorMessage = "Maksymalnie 100 znaków.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwa firmy jest wymagana.")]
        [StringLength(200, ErrorMessage = "Maksymalnie 200 znaków.")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format email.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "NIP jest wymagany.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "NIP musi zawierać 10 cyfr.")]
        public string Nip { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numer konta jest wymagany.")]
        [StringLength(34, MinimumLength = 16, ErrorMessage = "Numer konta musi mieć od 16 do 34 znaków.")]
        public string BankAccountNumber { get; set; } = string.Empty;

        public UpdateAddressDto Address { get; set; } = new();
    }
}
