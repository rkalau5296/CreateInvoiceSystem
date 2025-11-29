namespace CreateInvoiceSystem.Clients.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;

public class UpdateClientCommand : CommandBase<UpdateClientDto, UpdateClientDto>
{
    public override async Task<UpdateClientDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var client = await context.Set<Client>()
            .Include(c => c.Address)
            .Include(u => u.User)
            .FirstOrDefaultAsync(c => c.ClientId == Parametr.ClientId, cancellationToken: cancellationToken)
            
            ?? throw new InvalidOperationException($"Client with ID {Parametr.ClientId} not found.");        

        client.Name = this.Parametr.Name ?? client.Name;  
        client.Nip = this.Parametr.Nip ?? client.Nip;

        client.Address.Street = this.Parametr.Address?.Street ?? client.Address.Street;
        client.Address.Number = this.Parametr.Address?.Number ?? client.Address.Number;
        client.Address.City = this.Parametr.Address?.City ?? client.Address.City;
        client.Address.PostalCode = this.Parametr.Address?.PostalCode ?? client.Address.PostalCode;
        client.Address.Country = this.Parametr.Address?.Country ?? client.Address.Country;

        await context.SaveChangesAsync(cancellationToken);        
        return ClientMappers.ToUpdateDto(client);
    }
}
