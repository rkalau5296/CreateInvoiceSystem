namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;

public record UpdateUserDto(
    int UserId,
    string Name,
    string CompanyName,
    string Email,
    string Password,
    string Nip,
    UpdateAddressDto Address
);