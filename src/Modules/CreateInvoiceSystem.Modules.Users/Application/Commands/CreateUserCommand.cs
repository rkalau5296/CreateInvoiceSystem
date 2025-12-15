namespace CreateInvoiceSystem.Modules.Users.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Users.Dto;
using CreateInvoiceSystem.Modules.Users.Entities;
using CreateInvoiceSystem.Modules.Users.Mappers;
using Microsoft.EntityFrameworkCore;

public class CreateUserCommand : CommandBase<CreateUserDto, CreateUserDto>
{
    public override async Task<CreateUserDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));
        if (this.Parametr.Address is null)
            throw new ArgumentNullException(nameof(this.Parametr.Address));
        
        var entity = UserMappers.ToEntity(this.Parametr);

        await context.Set<User>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var persisted = await context.Set<User>()
            .AsNoTracking()
            .Include(u => u.Address)
            .SingleOrDefaultAsync(u => u.UserId == entity.UserId, cancellationToken);

        return persisted is not null
            ? UserMappers.ToCreateUserDto(persisted)
            : throw new InvalidOperationException("User was saved but could not be reloaded.");
    }
}
