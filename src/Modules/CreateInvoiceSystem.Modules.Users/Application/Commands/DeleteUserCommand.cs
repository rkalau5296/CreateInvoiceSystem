namespace CreateInvoiceSystem.Modules.Users.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Entities;
using CreateInvoiceSystem.Modules.Clients.Entities;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using CreateInvoiceSystem.Modules.Users.Dto;
using CreateInvoiceSystem.Modules.Users.Entities;
using CreateInvoiceSystem.Modules.Users.Mappers;
using Microsoft.EntityFrameworkCore;

public class DeleteUserCommand : CommandBase<User, UserDto>
{
    public override async Task<UserDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(context)); 

        var userEntity = await context.Set<User>()
            .Include(c => c.Address) 
            .FirstOrDefaultAsync(a => a.UserId == Parametr.UserId, cancellationToken: cancellationToken) ??
                              throw new InvalidOperationException($"User with ID {Parametr.UserId} not found.");

        context.Set<User>().Remove(userEntity);
        if (userEntity.Address is not null)
            context.Set<Address>().Remove(userEntity.Address);

        await context.SaveChangesAsync(cancellationToken);

        bool addrExists = await context.Set<Address>()
            .AsNoTracking()
            .AnyAsync(a => a.AddressId == userEntity.Address.AddressId, cancellationToken);

        bool userExists = await context.Set<User>()
            .AsNoTracking()
            .AnyAsync(c => c.UserId == userEntity.UserId, cancellationToken);

        return (!userExists && !addrExists)
            ? UserMappers.ToDto(userEntity)
            : throw new InvalidOperationException($"Failed to delete User or User address with ID {Parametr.UserId}.");
    }
}