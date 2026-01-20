namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;
public record UpdateClientDto(
    int ClientId,      
    string Name,      
    string Nip,
    AddressDto Address,
    int AddressId,    
    int UserId         
);
