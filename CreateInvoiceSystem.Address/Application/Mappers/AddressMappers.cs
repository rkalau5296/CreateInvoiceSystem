namespace CreateInvoiceSystem.Address.Application.Mappers;

using CreateInvoiceSystem.Address.Application.DTO;
using CreateInvoiceSystem.Address.Domain.Entities;

public static class AddressMappers 
{
    public static AddressDto ToDto(this Address address) =>
        address is null
            ? throw new ArgumentNullException(nameof(address))
            : new(address.AddressId, address.Street, address.Number, address.City, address.PostalCode, address.Email);

    public static Address ToEntity(this AddressDto dto) =>
        dto is null
            ? throw new ArgumentNullException(nameof(dto))
            : new Address
            {
                AddressId = dto.AddressId,
                Street = dto.Street,
                Number = dto.Number,
                City = dto.City,
                PostalCode = dto.PostalCode,
                Email = dto.Email
            };

    public static List<AddressDto> ToDtoList(this IEnumerable<Address> addresses) =>
        addresses is null
            ? throw new ArgumentNullException(nameof(addresses))
            : addresses.Select(a => a.ToDto()).ToList();
}
