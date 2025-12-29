using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;

namespace CreateInvoiceSystem.Modules.Users.Domain.Mappers;

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
            Address = dto.Address.ToEntity()
        };

    public static CreateUserDto ToCreateUserDto(this User user) =>
        user == null
        ? throw new ArgumentNullException(nameof(user), "User cannot be null when mapping to UserDto.")
        :
        new(user.Name, user.CompanyName, user.Email, user.Password, user.Nip, user.Address != null ? AddressMappers.ToCreateAddressDto(user.Address) : null);

    public static InvoiceDto ToDto(this Invoice invoice) =>
        invoice == null
        ? throw new ArgumentNullException(nameof(invoice), "Invoice cannot be null when mapping to InvoiceDto.")
        :
        new(
            invoice.InvoiceId,
            invoice.Title,
            invoice.TotalAmount,
            invoice.PaymentDate,
            invoice.CreatedDate,
            invoice.Comments,
            invoice.ClientId,
            invoice.UserId,
            invoice.MethodOfPayment,
            invoice.InvoicePositions.Select(ip => ip.ToDto()).ToList(),
            invoice.ClientName,
            invoice.ClientNip,
            invoice.ClientAddress
            );    

    //public static Address ToEntity(this AddressDto dto) =>
    //    dto == null
    //    ? throw new ArgumentNullException(nameof(dto), "Address cannot be null when mapping to Address.")
    //    :
    //    new()
    //    {
    //        AddressId = dto.AddressId,
    //        Street = dto.Street,
    //        Number = dto.Number,
    //        City = dto.City,
    //        PostalCode = dto.PostalCode,
    //        Country = dto.Country
    //    };
}