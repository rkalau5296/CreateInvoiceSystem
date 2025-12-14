using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Entities;
using CreateInvoiceSystem.Modules.Clients.Dto;
using CreateInvoiceSystem.Modules.Clients.Entities;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using CreateInvoiceSystem.Modules.Clients.Services;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Clients.Application.Commands;

public class DeleteClientCommand : CommandBase<Client, ClientDto>
{
    public override async Task<ClientDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var clientEntity = await context.Set<Client>()
            .Include(c => c.Address)
            .FirstOrDefaultAsync(a => a.ClientId == Parametr.ClientId, cancellationToken: cancellationToken) ??
                              throw new InvalidOperationException($"Client with ID {Parametr.ClientId} not found.");        

        context.Set<Client>().Remove(clientEntity);
        context.Set<Address>().Remove(clientEntity.Address);

        await context.SaveChangesAsync(cancellationToken);

        return ClientMappers.ToDto(clientEntity); ;
    }
}
