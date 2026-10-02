namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;

public record UserDto(
    int UserId,
    string Name,
    string CompanyName,
    string Email,
    string Password,
    string Nip,    
    AddressDto Address,
    string BankAccountNumber,
    bool IsActive,
    IEnumerable<InvoiceDto> Invoices,
    IEnumerable<ClientDto> Clients,
    IEnumerable<ProductDto> Products
 );
