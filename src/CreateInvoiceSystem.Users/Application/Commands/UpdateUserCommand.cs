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
        return UserMappers.ToUpdateUserDto(user);
    }
}
