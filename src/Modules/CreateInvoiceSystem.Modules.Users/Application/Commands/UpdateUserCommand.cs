namespace CreateInvoiceSystem.Modules.Users.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Users.Dto;
using CreateInvoiceSystem.Modules.Users.Entities;
using CreateInvoiceSystem.Modules.Users.Mappers;
using Microsoft.EntityFrameworkCore;

public class UpdateUserCommand : CommandBase<UpdateUserDto, UpdateUserDto>
{
    public override async Task<UpdateUserDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var user = await context.Set<User>()
            .Include(u => u.Address)
            .FirstOrDefaultAsync(c => c.UserId == Parametr.UserId, cancellationToken: cancellationToken)            
            ?? throw new InvalidOperationException($"User with ID {Parametr.UserId} not found.");
        
        string oldName = user.Name;
        string oldCompanyName = user.CompanyName;
        string oldEmail = user.Email;
        string oldPassword = user.Password;
        string oldNip = user.Nip;
        string oldStreet = user.Address?.Street;
        string oldNumber = user.Address?.Number;
        string oldCity = user.Address?.City;
        string oldPostal = user.Address?.PostalCode;
        string oldCountry = user.Address?.Country;

        user.Name = this.Parametr.Name ?? user.Name;
        user.CompanyName = this.Parametr.CompanyName ?? user.CompanyName;
        user.Email = this.Parametr.Email ?? user.Email;
        user.Password = this.Parametr.Password ?? user.Password;
        user.Nip = this.Parametr.Nip ?? user.Nip;

        user.Address.Street = this.Parametr.Address?.Street ?? user.Address.Street;
        user.Address.Number = this.Parametr.Address?.Number ?? user.Address.Number;
        user.Address.City = this.Parametr.Address?.City ?? user.Address.City;
        user.Address.PostalCode = this.Parametr.Address?.PostalCode ?? user.Address.PostalCode;
        user.Address.Country = this.Parametr.Address?.Country ?? user.Address.Country;    

        await context.SaveChangesAsync(cancellationToken);

        var persisted = await context.Set<User>()
            .AsNoTracking()
            .Include(u => u.Address)
            .SingleOrDefaultAsync(u => u.UserId == user.UserId, cancellationToken);

        bool hasChanged = persisted is not null && (
            !string.Equals(oldName, persisted.Name, StringComparison.Ordinal) ||
            !string.Equals(oldCompanyName, persisted.CompanyName, StringComparison.Ordinal) ||
            !string.Equals(oldEmail, persisted.Email, StringComparison.Ordinal) ||
            !string.Equals(oldPassword, persisted.Password, StringComparison.Ordinal) ||
            !string.Equals(oldNip, persisted.Nip, StringComparison.Ordinal) ||
            !string.Equals(oldStreet, persisted.Address?.Street, StringComparison.Ordinal) ||
            !string.Equals(oldNumber, persisted.Address?.Number, StringComparison.Ordinal) ||
            !string.Equals(oldCity, persisted.Address?.City, StringComparison.Ordinal) ||
            !string.Equals(oldPostal, persisted.Address?.PostalCode, StringComparison.Ordinal) ||
            !string.Equals(oldCountry, persisted.Address?.Country, StringComparison.Ordinal)
        );

        return hasChanged
            ? UserMappers.ToUpdateUserDto(persisted!)
            : throw new InvalidOperationException($"No changes were saved for user with ID {Parametr.UserId}.");
    }
}
