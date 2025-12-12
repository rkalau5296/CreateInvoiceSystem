using CreateInvoiceSystem.Modules.Addresses.Mappers;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using CreateInvoiceSystem.Modules.Invoices.Mappers;
using CreateInvoiceSystem.Modules.Products.Mappers;
using CreateInvoiceSystem.Modules.Users.Dto;
using CreateInvoiceSystem.Modules.Users.Entities;

namespace CreateInvoiceSystem.Modules.Users.Mappers;

public static class UserMappers
{
    public static UserDto ToDto(this User user) =>
        user == null
        ? throw new ArgumentNullException(nameof(user), "User cannot be null when mapping to UserDto.")
        :
        new(user.UserId, user.Name, user.CompanyName, user.Email, user.Password, user.Nip,  user.Address.ToDto(), user.Invoices.Select(i => i.ToDto()), user.Clients.Select(c => c.ToDto()), user.Products.Select(p => p.ToDto()));

    public static UpdateUserDto ToUpdateUserDto(this User user) =>
        user == null
        ? throw new ArgumentNullException(nameof(user), "User cannot be null when mapping to UserDto.")
        :
        new(user.UserId, user.Name, user.CompanyName, user.Email, user.Password, user.Nip, user.Address.ToUpdateAddressDto());

    public static User ToEntity(this UserDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "UserDto cannot be null when mapping to User.")
        :
        new()
        {
            UserId = dto.UserId,
            Name = dto.Name,
            Email = dto.Email,
            Password = dto.Password,
            Nip = dto.Nip,            
            Address = dto.Address.ToEntity(),
            Invoices = [.. dto.Invoices.Select(i => i.ToEntity())],
            Clients = [.. dto.Clients.Select(c => c.ToEntity())],
            Products = [.. dto.Products.Select(p => p.ToEntity())]
        };

    public static List<UserDto> ToDtoList(this ICollection<User> users) =>
        users == null
        ? throw new ArgumentNullException(nameof(users), "Users list cannot be null when mapping to UsersDto list.")
        :
         [.. users.Select(a => a.ToDto())];

    public static User ToEntity(CreateUserDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "UserDto cannot be null when mapping to User.")
        : new() 
        {
            Name = dto.Name,
            CompanyName = dto.CompanyName,
            Email = dto.Email,
            Password = dto.Password,
            Nip = dto.Nip,
            Address = dto.Address != null ? AddressMappers.ToEntity(dto.Address) : null
        };

    public static CreateUserDto ToCreateUserDto(this User user) =>
        user == null
        ? throw new ArgumentNullException(nameof(user), "User cannot be null when mapping to UserDto.")
        :
        new(user.Name, user.CompanyName, user.Email, user.Password, user.Nip, user.Address != null ? AddressMappers.ToCreateAddressDto(user.Address) : null);
}