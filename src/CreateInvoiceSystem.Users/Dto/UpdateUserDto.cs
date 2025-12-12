using CreateInvoiceSystem.Modules.Addresses.Dto;

namespace CreateInvoiceSystem.Modules.Users.Dto;

public record UpdateUserDto(
    int UserId,
    string Name,
    string CompanyName,
    string Email,
    string Password,
    string Nip,
    UpdateAddressDto Address
);