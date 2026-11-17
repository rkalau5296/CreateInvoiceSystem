namespace CreateInvoiceSystem.Modules.Invoices.Dto;
public record CreateClientDto(
    string Name,
    string Nip,
    AddressDto Address,
    int UserId,
    string Email
);
