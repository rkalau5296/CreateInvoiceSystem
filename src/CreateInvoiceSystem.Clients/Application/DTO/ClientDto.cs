using CreateInvoiceSystem.Abstractions.EntitiesBases;

namespace CreateInvoiceSystem.Clients.Application.DTO;

public record ClientDto(
    int ClientId,   
    string Name,    
    int AddressId,    
    string Email,        
    int UserId,
    AddressBase Address
);
