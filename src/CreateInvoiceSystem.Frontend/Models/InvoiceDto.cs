using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Frontend.Models
{
    public class InvoiceDto : IValidatableObject
    {
        public int InvoiceId { get; set; }

        [Required(ErrorMessage = "Numer faktury jest wymagany")]
        public string Title { get; set; } = string.Empty;

        public string Comments { get; set; } = string.Empty;

        [Required(ErrorMessage = "Metoda płatności jest wymagana")]
        public string MethodOfPayment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Termin płatności jest wymagany")]
        public DateTime PaymentDate { get; set; } = DateTime.Today.AddDays(14);

        [Required(ErrorMessage = "Data wystawienia jest wymagana")]
        public DateTime CreatedDate { get; set; } = DateTime.Today;

        public int UserId { get; set; }
        public string? UserEmail { get; set; }
        public int? ClientId { get; set; }

        [Required(ErrorMessage = "Nazwa sprzedawcy jest wymagana")]
        public string? SellerName { get; set; }

        [Required(ErrorMessage = "NIP sprzedawcy jest wymagany")]
        public string? SellerNip { get; set; }

        [Required(ErrorMessage = "Adres sprzedawcy jest wymagany")]
        public string? SellerAddress { get; set; }

        public string BankAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwa klienta jest wymagana")]
        public string? ClientName { get; set; }

        [Required(ErrorMessage = "NIP klienta jest wymagany")]
        public string? ClientNip { get; set; }

        [Required(ErrorMessage = "Adres klienta jest wymagany")]
        public string? ClientAddress { get; set; }

        public ClientDto? Client { get; set; }

        [MinLength(1, ErrorMessage = "Faktura musi mieć co najmniej jedną pozycję")]
        public List<InvoicePositionDto> InvoicePositions { get; set; } = new();

        public decimal TotalNet { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalGross { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (CreatedDate.Date > DateTime.Today)
                results.Add(new ValidationResult("Data wystawienia nie może być datą przyszłą.", new[] { nameof(CreatedDate) }));

            if (PaymentDate.Date < CreatedDate.Date)
                results.Add(new ValidationResult("Termin płatności nie może być wcześniejszy niż data wystawienia.", new[] { nameof(PaymentDate) }));

            return results;
        }
    }
}