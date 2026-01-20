namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;
public record ClientDto(
    int ClientId,   
    string Name,
    string Nip,
    AddressDto AddressDto,
    int UserId,
    bool IsDeleted
);
