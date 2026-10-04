namespace CreateInvoiceSystem.Modules.Users.Dto;

public record UpdateUserDto(
    int UserId,
    string Name,
    string CompanyName,
    string Email,    
    string Nip,
    string BankAccountNumber,    
    UpdateAddressDto Address
);