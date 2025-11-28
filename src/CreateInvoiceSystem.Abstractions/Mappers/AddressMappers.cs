namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class AddressMappers
{
    public static AddressDto ToDto(this Address address) =>
        new(address.AddressId, address.Street, address.Number, address.City, address.PostalCode,  address.Country, address.UserId);

    public static UpdateAddressDto ToUpdateAddressDto(this Address address) =>
        new(address.Street, address.Number, address.City, address.PostalCode, address.Country);
    
    public static Address ToEntity(this AddressDto dto) =>
        new()
        {
            AddressId = dto.AddressId,
            Street = dto.Street,
            Number = dto.Number,
            City = dto.City,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            UserId = dto.UserId,            
        };

    public static Address ToEntity(CreateAddressDto dto)
    {
        if (dto is null) return null;
        return new Address
        {            
            Street = dto.Street,
            Number = dto.Number,
            City = dto.City,
            PostalCode = dto.PostalCode,            
            Country = dto.Country          
        };
    }

    public static CreateAddressDto ToCreateAddressDto(Address address)
    {
        return new CreateAddressDto(
            address.Street,
            address.Number,
            address.City,
            address.PostalCode,
            address.Country
        );
    }


    public static List<AddressDto> ToDtoList(this IEnumerable<Address> addresses) =>
         [.. addresses.Select(a => a.ToDto())];
}
