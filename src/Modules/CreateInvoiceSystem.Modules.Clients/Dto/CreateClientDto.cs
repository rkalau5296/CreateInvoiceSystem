namespace CreateInvoiceSystem.Modules.Clients.Dto;
public record CreateClientDto(
    string Name,
    string Nip,
    AddressDto Address,
    int UserId, 
    string Email
);
