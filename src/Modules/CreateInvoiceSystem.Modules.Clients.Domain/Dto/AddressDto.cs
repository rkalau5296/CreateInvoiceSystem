namespace CreateInvoiceSystem.Modules.Clients.Domain.Dto;
public record AddressDto(
    int AddressId,   
    string Street,    
    string Number,    
    string City,    
    string PostalCode,
    string Country    
);
