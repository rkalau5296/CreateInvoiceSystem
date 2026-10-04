namespace CreateInvoiceSystem.Modules.Users.Dto;
public record CreateClientDto(
    string Name,
    string Nip,
    AddressDto Address,
    int UserId, 
    string Email
);
