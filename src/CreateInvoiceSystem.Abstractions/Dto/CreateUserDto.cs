namespace CreateInvoiceSystem.Abstractions.Dto;
public record CreateUserDto(
    string Name,
    string CompanyName,
    string Email,
    string Password,
    string Nip,
    CreateAddressDto Address
);