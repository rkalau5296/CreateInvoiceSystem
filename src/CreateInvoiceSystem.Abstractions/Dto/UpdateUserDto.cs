namespace CreateInvoiceSystem.Abstractions.Dto;

public record UpdateUserDto(
    int UserId,
    string Name,
    string CompanyName,
    string Email,
    string Password,
    string Nip,
    UpdateAddressDto Address
);