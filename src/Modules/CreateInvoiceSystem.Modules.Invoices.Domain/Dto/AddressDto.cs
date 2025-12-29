namespace CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
public record AddressDto(
    int AddressId,   
    string Street,    
    string Number,    
    string City,    
    string PostalCode,
    string Country    
);
