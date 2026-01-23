using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;

namespace CreateInvoiceSystem.Modules.Users.Domain.Mappers;

public static class UserMappers
{
    public static UserDto ToDto(this User user) =>
        user == null
        ? throw new ArgumentNullException(nameof(user), "User cannot be null when mapping to UserDto.")
        :
        new(
            user.UserId, 
            user.Name, 
            user.CompanyName, 
            user.Email, 
            user.Password,
            user.Nip, 
            user.Address.ToDto(),
            user.BankAccountNumber,
            user.Invoices.Select(i => i.ToDto()), 
            user.Clients.Select(c => c.ToDto()), 
            user.Products.Select(p => p.ToDto()));

    public static UpdateUserDto ToUpdateUserDto(this User user) =>
        user == null
        ? throw new ArgumentNullException(nameof(user), "User cannot be null when mapping to UserDto.")
        :
        new(user.UserId, user.Name, user.CompanyName, user.Email, user.Nip, user.BankAccountNumber, user.Address.ToUpdateAddressDto());

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
            CompanyName = dto.CompanyName,
            BankAccountNumber = dto.BankAccountNumber,
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
        invoice.TotalNet,
        invoice.TotalVat,
        invoice.TotalGross,
        invoice.PaymentDate,
        invoice.CreatedDate,
        invoice.Comments,
        invoice.ClientId,
        invoice.UserId,
        invoice.MethodOfPayment,
        invoice.InvoicePositions.Select(ip => ip.ToDto()).ToList(),
        invoice.SellerName,
        invoice.SellerNip,
        invoice.SellerAddress,
        invoice.BankAccountNumber,
        invoice.ClientName,
        invoice.ClientNip,
        invoice.ClientAddress
    );

    public static User ToEntity(RegisterUserDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "User cannot be null when mapping to UserDto.")
        :
        new User
        {
            Email = dto.Email,
            Name = dto.Name,
            CompanyName = dto.CompanyName,
            Nip = dto.Nip,
            BankAccountNumber = dto.BankAccountNumber,
            Address = new Address
            {
                Street = dto.Address.Street,
                Number = dto.Address.Number,
                City = dto.Address.City,
                PostalCode = dto.Address.PostalCode,
                Country = dto.Address.Country
            }
        };

    public static RegisterUserDto ToRegisterUserDto(this User entity) =>
        entity == null
        ? throw new ArgumentNullException(nameof(entity), "User cannot be null when mapping to UserDto.")
        :
        new RegisterUserDto
        {
            Email = entity.Email,
            Name = entity.Name,
            CompanyName = entity.CompanyName,
            Nip = entity.Nip,
            BankAccountNumber = entity.BankAccountNumber,
            Address = new RegisterAddressDto
            {
                Street = entity.Address?.Street ?? string.Empty,
                Number = entity.Address?.Number ?? string.Empty,
                City = entity.Address?.City ?? string.Empty,
                PostalCode = entity.Address?.PostalCode ?? string.Empty,
                Country = entity.Address?.Country ?? "Poland"
            }
        };
}
