namespace CreateInvoiceSystem.Address.Application.Mappers;

using CreateInvoiceSystem.Address.Application.DTO;
using CreateInvoiceSystem.Address.Domain.Entities;

public static class AddressMappers 
{
    public static AddressDto ToDto(this Address e) =>
        new (e.AddressId, e.Street, e.Number, e.City, e.PostalCode, e.Email);

    public static Address ToEntity(this AddressDto dto) => 
        new ( dto.AddressId, dto.Street, dto.Number, dto.City, dto.PostalCode, dto.Email);
    
}
