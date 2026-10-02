namespace CreateInvoiceSystem.Modules.Invoices.Domain.Dto;

public record UpdateClientDto
(
    int ClientId,
    string Name,
    string Nip,
    AddressDto Address,
    int UserId, 
    string Email
);
