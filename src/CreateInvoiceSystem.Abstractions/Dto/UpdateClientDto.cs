namespace CreateInvoiceSystem.Abstractions.Dto;

public record UpdateClientDto(
    int ClientId,      
    string Name,      
    string Nip,
    AddressDto Address,
    int AddressId,    
    int UserId         
);
