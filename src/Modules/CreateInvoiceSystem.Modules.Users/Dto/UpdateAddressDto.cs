namespace CreateInvoiceSystem.Modules.Users.Dto;
public record UpdateAddressDto(
    string Street,
    string Number,
    string City,
    string PostalCode,
    string Country            
    );
