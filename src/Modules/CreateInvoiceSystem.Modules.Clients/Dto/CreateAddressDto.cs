namespace CreateInvoiceSystem.Modules.Clients.Dto;
public record CreateAddressDto(
    string Street,
    string Number,
    string City,
    string PostalCode,
    string Country
);
