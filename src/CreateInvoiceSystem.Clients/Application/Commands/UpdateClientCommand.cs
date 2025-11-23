namespace CreateInvoiceSystem.Clients.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.DTO;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Clients.Application.Mappers;
using Microsoft.EntityFrameworkCore;

public class UpdateClientCommand : CommandBase<ClientDto, ClientDto>
{
    public override async Task<ClientDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var client = await context.Set<Client>().FirstOrDefaultAsync(a => a.ClientId == Parametr.ClientId, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Address with ID {Parametr.ClientId} not found.");

        client.Name = this.Parametr.Name;
        client.Email = this.Parametr.Email;
        client.Address = this.Parametr.Address;        

        await context.SaveChangesAsync(cancellationToken);        
        return ClientMappers.ToDto(client);
    }
}
