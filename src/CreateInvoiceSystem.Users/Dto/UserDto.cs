using CreateInvoiceSystem.Modules.Addresses.Dto;
using CreateInvoiceSystem.Modules.Clients.Dto;
using CreateInvoiceSystem.Modules.Invoices.Dto;
using CreateInvoiceSystem.Modules.Products.Dto;

namespace CreateInvoiceSystem.Modules.Users.Dto;

public record UserDto(
    int UserId,
    string Name,
    string CompanyName,
    string Email,
    string Password,
    string Nip,    
    AddressDto Address,
    IEnumerable<InvoiceDto> Invoices,
    IEnumerable<ClientDto> Clients,
    IEnumerable<ProductDto> Products
 );
