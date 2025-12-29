namespace CreateInvoiceSystem.Modules.Clients.Domain.Dto;
public record UpdateAddressDto(
    string Street,
    string Number,
    string City,
    string PostalCode,
    string Country            
    );
