using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
public class UpdateClientCommand : CommandBase<UpdateClientDto, UpdateClientDto, IClientRepository>
{
    public override async Task<UpdateClientDto> Execute(IClientRepository _clientRepository, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(Parametr));

        var client = await _clientRepository.GetByIdAsync(Parametr.ClientId, includeAddress: true, cancellationToken)
            ?? throw new InvalidOperationException($"Client with ID {Parametr.ClientId} not found.");
        
        string oldName = client.Name;
        string oldNip = client.Nip;
        string oldStreet = client.Address.Street;
        string oldNumber = client.Address.Number;
        string oldCity = client.Address.City;
        string oldPostal = client.Address.PostalCode;
        string oldCountry = client.Address.Country;

        client.Name = Parametr.Name ?? client.Name;
        client.Nip = Parametr.Nip ?? client.Nip;

        if (client.Address is null && Parametr.Address is not null)
        {
            client.Address = new Address
            {
                Street = Parametr.Address.Street,
                Number = Parametr.Address.Number,
                City = Parametr.Address.City,
                PostalCode = Parametr.Address.PostalCode,
                Country = Parametr.Address.Country
            };
        }
        else if (client.Address is not null)
        {
            client.Address.Street = Parametr.Address?.Street ?? client.Address.Street;
            client.Address.Number = Parametr.Address?.Number ?? client.Address.Number;
            client.Address.City = Parametr.Address?.City ?? client.Address.City;
            client.Address.PostalCode = Parametr.Address?.PostalCode ?? client.Address.PostalCode;
            client.Address.Country = Parametr.Address?.Country ?? client.Address.Country;
        }

        await _clientRepository.UpdateAsync(client, cancellationToken);
        await _clientRepository.SaveChangesAsync(cancellationToken);

        var persisted = await _clientRepository.GetByIdAsync(client.ClientId, includeAddress: true, cancellationToken);

        bool hasChanged = persisted is not null && (
            !string.Equals(oldName, persisted.Name, StringComparison.Ordinal) ||
            !string.Equals(oldNip, persisted.Nip, StringComparison.Ordinal) ||
            !string.Equals(oldStreet, persisted.Address?.Street, StringComparison.Ordinal) ||
            !string.Equals(oldNumber, persisted.Address?.Number, StringComparison.Ordinal) ||
            !string.Equals(oldCity, persisted.Address?.City, StringComparison.Ordinal) ||
            !string.Equals(oldPostal, persisted.Address?.PostalCode, StringComparison.Ordinal) ||
            !string.Equals(oldCountry, persisted.Address?.Country, StringComparison.Ordinal)
        );

        return hasChanged
            ? ClientMappers.ToUpdateDto(persisted!)
            : throw new InvalidOperationException($"No changes were saved for client with ID {Parametr.ClientId}.");
    }
}
