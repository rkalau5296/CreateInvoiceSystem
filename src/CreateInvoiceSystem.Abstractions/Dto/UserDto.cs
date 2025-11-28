namespace CreateInvoiceSystem.Abstractions.Dto;

public record UserDto(
    int UserId,
    string Name,
    string CompanyName,
    string Email,
    string Password,
    string Nip,
    int AddressId,
    AddressDto AddressDto,
    IEnumerable<InvoiceDto> Invoices,
    IEnumerable<ClientDto> Clients,
    IEnumerable<ProductDto> Products
 );
