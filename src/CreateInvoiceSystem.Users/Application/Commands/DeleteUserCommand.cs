namespace CreateInvoiceSystem.Modules.Users.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Entities;
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

        var UserDto = UserMappers.ToDto(userEntity);

        if (userEntity.Address is null)
            throw new ArgumentNullException(nameof(userEntity.Address));

        context.Set<User>().Remove(userEntity);
        context.Set<Address>().Remove(userEntity.Address);        
        
        await context.SaveChangesAsync(cancellationToken);

        return UserDto;
    }
}