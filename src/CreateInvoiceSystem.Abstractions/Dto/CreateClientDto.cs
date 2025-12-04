namespace CreateInvoiceSystem.Abstractions.Dto;

public record CreateClientDto(
    string Name,
    string Nip,
    AddressDto Address,
    int UserId,
    bool IsDeleted
);
