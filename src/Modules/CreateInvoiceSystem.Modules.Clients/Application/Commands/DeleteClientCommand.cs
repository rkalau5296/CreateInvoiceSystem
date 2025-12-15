using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Entities;
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
            .FirstOrDefaultAsync(a => a.ClientId == Parametr.ClientId, cancellationToken: cancellationToken) ??
                              throw new InvalidOperationException($"Client with ID {Parametr.ClientId} not found.");

        if (clientEntity.Address is not null)
            context.Set<Address>().Remove(clientEntity.Address);

        context.Set<Client>().Remove(clientEntity);        

        await context.SaveChangesAsync(cancellationToken);

        var stillExists = await context.Set<Client>()
            .AsNoTracking()
            .AnyAsync(c => c.ClientId == clientEntity.ClientId, cancellationToken);

        var addrExists = await context.Set<Address>()
            .AsNoTracking()
            .AnyAsync(a => a.AddressId == clientEntity.Address.AddressId, cancellationToken);        

        return (!stillExists && !addrExists)
            ? ClientMappers.ToDto(clientEntity)
            : throw new InvalidOperationException($"Failed to delete Client or Client address with ID {Parametr.ClientId}.");
    }
}
