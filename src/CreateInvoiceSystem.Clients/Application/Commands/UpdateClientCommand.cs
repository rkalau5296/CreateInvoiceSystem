namespace CreateInvoiceSystem.Clients.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;

public class UpdateClientCommand : CommandBase<ClientDto, ClientDto>
{
    public override async Task<ClientDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var client = await context.Set<Client>()
            .Include(c => c.Address)
            .FirstOrDefaultAsync(c => c.ClientId == Parametr.ClientId, cancellationToken: cancellationToken)
            
            ?? throw new InvalidOperationException($"Client with ID {Parametr.ClientId} not found.");        

        client.Name = Parametr.Name;  
        client.AddressId = client.AddressId;
        //client.Address.Street = this.Parametr.AddressDto.Street;
        //client.Address.Number = this.Parametr.AddressDto.Number;
        //client.Address.City = this.Parametr.AddressDto.City;
        //client.Address.PostalCode = this.Parametr.AddressDto.PostalCode;        
        //client.Address.Country = this.Parametr.AddressDto.Country;

        await context.SaveChangesAsync(cancellationToken);        
        return ClientMappers.ToDto(client);
    }
}
