namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;

public record CreateUserDto(
    string Name,
    string CompanyName,
    string Email,
    string Password,
    string Nip,
    bool IsActive,
    CreateAddressDto Address
);