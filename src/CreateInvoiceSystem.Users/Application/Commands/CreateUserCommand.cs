namespace CreateInvoiceSystem.Users.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;

public class CreateUserCommand : CommandBase<CreateUserDto, CreateUserDto>
{
    public override async Task<CreateUserDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var entity = UserMappers.ToEntity(this.Parametr);

        await context.Set<User>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        this.Parametr = UserMappers.ToCreateUserDto(entity);
        return this.Parametr;
    }
}
