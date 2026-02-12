using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Frontend.Models;

public record RegisterUserRequest(RegisterUserDto User);

public record RegisterUserDto
{
    [Required(ErrorMessage = "Email jest wymagany.")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hasło jest wymagane.")]
    [MinLength(6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Imię i nazwisko jest wymagane.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwa firmy jest wymagana.")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "NIP jest wymagany.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "NIP musi składać się z 10 cyfr.")]
    public string Nip { get; set; } = string.Empty;

    [RegularExpression(@"^PL\d{26}$", ErrorMessage = "IBAN musi zaczynać się od PL i zawierać dokładnie 26 cyfr.")]
    public string? BankAccountNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adres jest wymagany.")]
    public RegisterAddressDto Address { get; set; } = new();
}

public record RegisterAddressDto
{
    [Required(ErrorMessage = "Ulica jest wymagana.")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numer budynku jest wymagany.")]
    public string Number { get; set; } = string.Empty;

    [Required(ErrorMessage = "Miasto jest wymagane.")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kod pocztowy jest wymagany.")]
    [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Kod pocztowy ma format XX-XXX.")]
    public string PostalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kraj jest wymagany.")]
    public string Country { get; set; } = "Poland";
}

public record RegisterUserResponse(RegisterUserDto? Data, bool Success, string Message);