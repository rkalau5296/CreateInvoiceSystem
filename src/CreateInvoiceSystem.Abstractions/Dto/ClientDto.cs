using CreateInvoiceSystem.Abstractions.Entities;

namespace CreateInvoiceSystem.Abstractions.Dto;

public record ClientDto(
    int ClientId,   
    string Name,
    string Nip,
    int AddressId,
    AddressDto AddressDto,
    ICollection<Invoice> Invoices,
    int UserId,
    UserDto UserDto    
);
