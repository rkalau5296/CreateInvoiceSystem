namespace CreateInvoiceSystem.Frontend.Models;

public class UpdateAddressDto
{
    public string Street { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = "Polska";
}
