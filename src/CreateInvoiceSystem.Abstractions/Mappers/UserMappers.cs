namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class UserMappers
{
    public static UserDto ToDto(this User User) =>
        new(User.UserId, User.Name, User.Email, User.Password, User.Nip, User.AddressId, User.Address.ToDto());

    public static User ToEntity(this UserDto dto) =>
        new()
        {
            UserId = dto.UserId,
            Name = dto.Name,
            Email = dto.Email,
            Password = dto.Password,
            Nip = dto.Nip,
            AddressId = dto.AddressId,
            Address = dto.AddressDto.ToEntity()
        };

    public static List<UserDto> ToDtoList(this IEnumerable<User> users) =>
         [.. users.Select(a => a.ToDto())];
}
