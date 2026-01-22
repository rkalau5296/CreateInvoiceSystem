namespace CreateInvoiceSystem.Modules.Clients.Domain.Dto;
public record ClientDto(
    int ClientId,   
    string Name,
    string Nip,
    AddressDto Address,    
    int UserId,
    bool IsDeleted
);
