using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Modules.Clients.Dto;
using CreateInvoiceSystem.Modules.Clients.Entities;
using CreateInvoiceSystem.Modules.Clients.Mappers;
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
            .Include(u => u.User)
            .FirstOrDefaultAsync(a => a.ClientId == Parametr.ClientId, cancellationToken: cancellationToken) ??
                              throw new InvalidOperationException($"Client with ID {Parametr.ClientId} not found.");

        var clientDto = ClientMappers.ToDto(clientEntity);

        bool isUsed = await context.Set<Invoice>()
            .AnyAsync(i => i.ClientId == Parametr.ClientId, cancellationToken);

        if (!isUsed)
        {
            var clientAddress = clientEntity.Address ?? throw new InvalidOperationException($"Client with ID {Parametr.ClientId} not found.");
            context.Set<Client>().Remove(clientEntity);
            context.Set<Address>().Remove(clientEntity.Address);
        }
        else
        {
            clientEntity.IsDeleted = true;
            context.Set<Client>().Update(clientEntity);
        }
        await context.SaveChangesAsync(cancellationToken);

        return clientDto;
    }
}
