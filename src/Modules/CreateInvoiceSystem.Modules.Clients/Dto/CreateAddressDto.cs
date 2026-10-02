namespace CreateInvoiceSystem.Modules.Clients.Domain.Dto;
public record CreateAddressDto(
    string Street,
    string Number,
    string City,
    string PostalCode,
    string Country
);
