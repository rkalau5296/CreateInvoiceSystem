namespace CreateInvoiceSystem.Clients.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.DTO;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;

public class DeleteClientCommand : CommandBase<Client, ClientDto>
{
    public override async Task<ClientDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(context)); 

        var clientEntity = await context.Set<Client>()
            .Include(c => c.Address) 
            .FirstOrDefaultAsync(a => a.ClientId == Parametr.ClientId, cancellationToken: cancellationToken) ??
                              throw new InvalidOperationException($"Address with ID {Parametr.ClientId} not found.");

        var clientDto = ClientMappers.ToDto(clientEntity);
        
        var clientAddress = clientEntity.Address ?? throw new InvalidOperationException($"Address with ID {Parametr.ClientId} not found."); 
        
        context.Set<Address>().Remove(clientEntity.Address);
        context.Set<Client>().Remove(clientEntity);
        await context.SaveChangesAsync(cancellationToken);

        return clientDto;
    }
}
