using CreateInvoiceSystem.Modules.Addresses.Dto;
using CreateInvoiceSystem.Modules.Addresses.Entities;

namespace CreateInvoiceSystem.Modules.Addresses.Mappers;

public static class AddressMappers
{
    public static AddressDto ToDto(this Address address) =>
        address == null
        ? throw new ArgumentNullException(nameof(address), "Address cannot be null when mapping to AddressDto.")
        :
        new(address.AddressId, address.Street, address.Number, address.City, address.PostalCode, address.Country);

    public static UpdateAddressDto ToUpdateAddressDto(this Address address) =>
        address == null
        ? throw new ArgumentNullException(nameof(address), "Address cannot be null when mapping to UpdateAddressDto.")
        :
        new(address.Street, address.Number, address.City, address.PostalCode, address.Country);

    public static Address ToEntity(this AddressDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "Address cannot be null when mapping to Address.")
        :
        new()
        {
            AddressId = dto.AddressId,
            Street = dto.Street,
            Number = dto.Number,
            City = dto.City,
            PostalCode = dto.PostalCode,
            Country = dto.Country
        };

    public static Address ToEntity(CreateAddressDto dto) =>
        dto == null
       ? throw new ArgumentNullException(nameof(dto), "Address cannot be null when mapping to Address.")
       :
        new Address
        {
            Street = dto.Street,
            Number = dto.Number,
            City = dto.City,
            PostalCode = dto.PostalCode,
            Country = dto.Country
        };


    public static CreateAddressDto ToCreateAddressDto(Address address) =>
         address == null
        ? throw new ArgumentNullException(nameof(address), "Address cannot be null when mapping to Address.")
        :
         new (
            address.Street,
            address.Number,
            address.City,
            address.PostalCode,
            address.Country
        );
    


    public static List<AddressDto> ToDtoList(this IEnumerable<Address> addresses) =>
        addresses == null
        ? throw new ArgumentNullException(nameof(addresses), "Addresses cannot be null when mapping to List<AddressDto> .")
        :[.. addresses.Select(a => a.ToDto())];
}
