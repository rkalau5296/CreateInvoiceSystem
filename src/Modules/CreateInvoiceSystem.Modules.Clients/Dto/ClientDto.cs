using CreateInvoiceSystem.Modules.Addresses.Dto;

namespace CreateInvoiceSystem.Modules.Clients.Dto;

public record ClientDto(
    int ClientId,   
    string Name,
    string Nip,
    AddressDto AddressDto,
    int UserId,
    bool IsDeleted
);
