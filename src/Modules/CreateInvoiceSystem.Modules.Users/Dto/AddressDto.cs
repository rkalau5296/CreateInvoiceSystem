namespace CreateInvoiceSystem.Modules.Users.Dto;
public record AddressDto(
    int AddressId,   
    string Street,    
    string Number,    
    string City,    
    string PostalCode,
    string Country    
);
