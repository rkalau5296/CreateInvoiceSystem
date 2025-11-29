using CreateInvoiceSystem.Abstractions.Entities;

namespace CreateInvoiceSystem.Abstractions.Dto;

public record ClientDto(
    int ClientId,   
    string Name,
    string Nip,
    AddressDto Address,
    int UserId    
);
