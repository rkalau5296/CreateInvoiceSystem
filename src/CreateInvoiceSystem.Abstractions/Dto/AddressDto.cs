using CreateInvoiceSystem.Abstractions.Entities;

namespace CreateInvoiceSystem.Abstractions.Dto;

public record AddressDto(
    int AddressId,   
    string Street,    
    string Number,    
    string City,    
    string PostalCode,    
    string Email,
    string Country,
    int UserId,
    UserDto UserDto
);
