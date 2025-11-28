namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class UserMappers
{
    public static UserDto ToDto(this User user) =>
        new(user.UserId, user.Name, user.CompanyName, user.Email, user.Password, user.Nip, user.AddressId, user.Address.ToDto(), user.Invoices.Select(i => i.ToDto()), user.Clients.Select(c => c.ToDto()), user.Products.Select(p => p.ToDto()));


    public static User ToEntity(this UserDto dto) =>
        new()
        {
            UserId = dto.UserId,
            Name = dto.Name,
            Email = dto.Email,
            Password = dto.Password,
            Nip = dto.Nip,
            AddressId = dto.AddressId,
            Address = dto.AddressDto.ToEntity(),
            Invoices = [.. dto.Invoices.Select(i => i.ToEntity())],
            Clients = [.. dto.Clients.Select(c => c.ToEntity())],
            Products = [.. dto.Products.Select(p => p.ToEntity())]
        };

    public static List<UserDto> ToDtoList(this IEnumerable<User> users) =>
         [.. users.Select(a => a.ToDto())];

    public static User ToEntity(CreateUserDto dto)
    {
        if (dto is null) return null;
        return new User
        {
            Name = dto.Name,
            CompanyName = dto.CompanyName,
            Email = dto.Email,
            Password = dto.Password,
            Nip = dto.Nip,
            Address = dto.Address != null ? AddressMappers.ToEntity(dto.Address) : null
        };
    }

    public static CreateUserDto ToCreateUserDto(this User user) =>
        new(user.Name, user.CompanyName, user.Email, user.Password, user.Nip, user.Address != null ? AddressMappers.ToCreateAddressDto(user.Address) : null);
}