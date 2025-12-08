using CreateInvoiceSystem.Abstractions.Dto;

namespace CreateInvoiceSystem.Modules.Clients.Dto;

public record ClientDto(
    int ClientId,   
    string Name,
    string Nip,
    AddressDto Address,
    int UserId,
    bool IsDeleted
);
