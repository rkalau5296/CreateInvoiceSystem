namespace CreateInvoiceSystem.Address.Application.DTO;

public record AddressDto(
    int Id,
    string Street,
    string Number,
    string City,
    string PostalCode,
    string? Email
);
