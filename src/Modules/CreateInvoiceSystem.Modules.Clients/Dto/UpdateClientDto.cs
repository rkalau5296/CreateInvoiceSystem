using CreateInvoiceSystem.Abstractions.Dto;

namespace CreateInvoiceSystem.Modules.Clients.Dto;

public record UpdateClientDto(
    int ClientId,      
    string Name,      
    string Nip,
    AddressDto Address,
    int AddressId,    
    int UserId         
);
