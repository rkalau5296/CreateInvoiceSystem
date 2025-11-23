namespace CreateInvoiceSystem.Clients.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.DTO;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Clients.Application.Mappers;

public class CreateClientCommand : CommandBase<ClientDto, ClientDto>
{
    public override async Task<ClientDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var entity = ClientMappers.ToEntity(this.Parametr);

        await context.Set<Client>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        this.Parametr = ClientMappers.ToDto(entity);
        return this.Parametr;
    }
}
