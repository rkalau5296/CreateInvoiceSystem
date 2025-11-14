namespace CreateInvoiceSystem.Address.Application.DTO;

public record AddressDto(
    int AddressId,
    string Street,
    string Number,
    string City,
    string PostalCode,
    string Email
);
