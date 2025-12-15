using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Clients.Dto;
using CreateInvoiceSystem.Modules.Clients.Entities;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Clients.Application.Commands;

public class UpdateClientCommand : CommandBase<UpdateClientDto, UpdateClientDto>
{
    public override async Task<UpdateClientDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var client = await context.Set<Client>()
            .Include(c => c.Address)
            .FirstOrDefaultAsync(c => c.ClientId == Parametr.ClientId, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Client with ID {Parametr.ClientId} not found.");

        string oldName = client.Name;
        string oldNip = client.Nip;
        string oldStreet = client.Address?.Street;
        string oldNumber = client.Address?.Number;
        string oldCity = client.Address?.City;
        string oldPostal = client.Address?.PostalCode;
        string oldCountry = client.Address?.Country;

        client.Name = this.Parametr.Name ?? client.Name;
        client.Nip = this.Parametr.Nip ?? client.Nip;
        client.Address.Street = this.Parametr.Address?.Street ?? client.Address.Street;
        client.Address.Number = this.Parametr.Address?.Number ?? client.Address.Number;
        client.Address.City = this.Parametr.Address?.City ?? client.Address.City;
        client.Address.PostalCode = this.Parametr.Address?.PostalCode ?? client.Address.PostalCode;
        client.Address.Country = this.Parametr.Address?.Country ?? client.Address.Country;

        await context.SaveChangesAsync(cancellationToken);

        var persisted = await context.Set<Client>()
            .AsNoTracking()
            .Include(c => c.Address)
            .SingleOrDefaultAsync(c => c.ClientId == client.ClientId, cancellationToken);

        bool hasChanged = persisted is not null && (
            !string.Equals(oldName, persisted.Name, StringComparison.Ordinal) ||
            !string.Equals(oldNip, persisted.Nip, StringComparison.Ordinal) ||
            !string.Equals(oldStreet, persisted.Address?.Street, StringComparison.Ordinal) ||
            !string.Equals(oldNumber, persisted.Address?.Number, StringComparison.Ordinal) ||
            !string.Equals(oldCity, persisted.Address?.City, StringComparison.Ordinal) ||
            !string.Equals(oldPostal, persisted.Address?.PostalCode, StringComparison.Ordinal) ||
            !string.Equals(oldCountry, persisted.Address?.Country, StringComparison.Ordinal)
        );

        return persisted is not null
            ? ClientMappers.ToUpdateDto(persisted)
            : throw new InvalidOperationException($"No changes were saved for client with ID {Parametr.ClientId}.");
    }
}
