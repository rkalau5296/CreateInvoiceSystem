using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Addresses.Application.DTO;

public record AddressDto(
    int AddressId,   
    string Street,    
    string Number,    
    string City,    
    string PostalCode,    
    string Email,
    string Country
);
