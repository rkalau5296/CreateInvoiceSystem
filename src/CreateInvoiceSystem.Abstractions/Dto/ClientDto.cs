namespace CreateInvoiceSystem.Abstractions.DTO;

using CreateInvoiceSystem.Abstractions.Entities;

public record ClientDto(
    int ClientId,   
    string Name,
    string Nip,
    int AddressId,
    //int UserId,    
    AddressDto AddressDto
);
