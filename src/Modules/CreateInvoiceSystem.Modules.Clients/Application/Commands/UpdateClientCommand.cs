using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Clients.Dto;
using CreateInvoiceSystem.Modules.Clients.Entities;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using CreateInvoiceSystem.Modules.Clients.Services;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Clients.Application.Commands;

public class UpdateClientCommand(IInvoiceReadService invoiceRead) : CommandBase<UpdateClientDto, UpdateClientDto>
{
    public override async Task<UpdateClientDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var client = await context.Set<Client>()
            .Include(c => c.Address)            
            .FirstOrDefaultAsync(c => c.ClientId == Parametr.ClientId, cancellationToken: cancellationToken)     
            ?? throw new InvalidOperationException($"Client with ID {Parametr.ClientId} not found.");

        var isUsed = await invoiceRead.IsClientUsedAsync(Parametr.ClientId, cancellationToken);

        if (!isUsed)
        {
            client.Name = this.Parametr.Name ?? client.Name;
            client.Nip = this.Parametr.Nip ?? client.Nip;

            client.Address.Street = this.Parametr.Address?.Street ?? client.Address.Street;
            client.Address.Number = this.Parametr.Address?.Number ?? client.Address.Number;
            client.Address.City = this.Parametr.Address?.City ?? client.Address.City;
            client.Address.PostalCode = this.Parametr.Address?.PostalCode ?? client.Address.PostalCode;
            client.Address.Country = this.Parametr.Address?.Country ?? client.Address.Country;

            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            throw new InvalidOperationException("The client cannot be edited because it has already been used in documents. Please update data on a new client or create a new client.");
        }
        
        return ClientMappers.ToUpdateDto(client);
    }
}
