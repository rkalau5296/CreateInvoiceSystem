using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Frontend.Models;

public class UpdateAddressDto
{
    [Required(ErrorMessage = "Ulica jest wymagana.")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nr budynku jest wymagany.")]
    public string Number { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kod pocztowy jest wymagany.")]
    [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Kod pocztowy musi mieć format XX-XXX.")]
    public string PostalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Miasto jest wymagane.")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kraj jest wymagany.")]
    public string Country { get; set; } = "Polska";
}
