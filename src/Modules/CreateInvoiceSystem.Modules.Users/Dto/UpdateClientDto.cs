namespace CreateInvoiceSystem.Modules.Users.Dto;
public record UpdateClientDto(
    int ClientId,      
    string Name,      
    string Nip,
    string Email,
    AddressDto Address,
    int AddressId,    
    int UserId         
);
