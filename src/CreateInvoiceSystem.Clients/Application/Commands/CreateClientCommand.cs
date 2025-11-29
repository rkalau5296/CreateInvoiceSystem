namespace CreateInvoiceSystem.Clients.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;

public class CreateClientCommand : CommandBase<CreateClientDto, CreateClientDto>
{
    public override async Task<CreateClientDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var entity = ClientMappers.ToEntity(this.Parametr);

        await context.Set<Client>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        return this.Parametr;
    }
}
