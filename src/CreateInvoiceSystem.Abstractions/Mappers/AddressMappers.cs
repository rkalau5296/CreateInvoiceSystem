namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class AddressMappers
{
    public static AddressDto ToDto(this Address address) =>
        new(address.AddressId, address.Street, address.Number, address.City, address.PostalCode, address.Email, address.Country, address.UserId, address.User.ToDto());

    public static Address ToEntity(this AddressDto dto) =>
        new()
        {
            AddressId = dto.AddressId,
            Street = dto.Street,
            Number = dto.Number,
            City = dto.City,
            PostalCode = dto.PostalCode,            
            Email = dto.Email,
            Country = dto.Country,
            UserId = dto.UserId,
            User = dto.UserDto.ToEntity()
        };

    public static List<AddressDto> ToDtoList(this IEnumerable<Address> addresses) =>
         [.. addresses.Select(a => a.ToDto())];
}
