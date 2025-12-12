using CreateInvoiceSystem.Modules.Addresses.Dto;

namespace CreateInvoiceSystem.Modules.Users.Dto;

public record CreateUserDto(
    string Name,
    string CompanyName,
    string Email,
    string Password,
    string Nip,
    CreateAddressDto Address
);