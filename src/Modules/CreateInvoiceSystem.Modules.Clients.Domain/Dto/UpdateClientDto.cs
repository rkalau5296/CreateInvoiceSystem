namespace CreateInvoiceSystem.Modules.Clients.Domain.Dto;
public record UpdateClientDto(
    int ClientId,      
    string Name,      
    string Nip,    
    AddressDto Address,
    int AddressId,    
    int UserId         
);
