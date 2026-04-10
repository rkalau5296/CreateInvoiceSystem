namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;
public record CreateClientDto(
    string Name,
    string Nip,
    AddressDto Address,
    int UserId, 
    string Email
);
