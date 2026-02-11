namespace CreateInvoiceSystem.Frontend.Models;

public record RegisterUserRequest(RegisterUserDto User);

public record RegisterUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Nip { get; set; } = string.Empty;
    public string? BankAccountNumber { get; set; } = string.Empty;
    public RegisterAddressDto Address { get; set; } = new();
}

public record RegisterAddressDto
{
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "Poland";
}

public record RegisterUserResponse(RegisterUserDto? Data, bool Success, string Message);