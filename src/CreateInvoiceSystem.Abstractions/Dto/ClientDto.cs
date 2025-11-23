namespace CreateInvoiceSystem.Abstractions.DTO;

using CreateInvoiceSystem.Abstractions.Entities;

public record ClientDto(
    int ClientId,   
    string Name,    
    int AddressId,    
    string Email,        
    int UserId,
    Address Address
);
