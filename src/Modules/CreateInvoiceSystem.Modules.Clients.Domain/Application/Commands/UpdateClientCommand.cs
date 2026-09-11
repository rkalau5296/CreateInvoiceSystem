using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
public class UpdateClientCommand : CommandBase<UpdateClientDto, UpdateClientDto, IClientRepository>
{
    public override async Task<UpdateClientDto> Execute(IClientRepository clientRepository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Parametr);

        var client = await clientRepository.GetByIdAsync(Parametr.ClientId, Parametr.UserId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Client with ID {Parametr.ClientId} not found.");

        client.Name = Parametr.Name ?? client.Name;
        client.Nip = Parametr.Nip ?? client.Nip;
        client.Email = Parametr.Email ?? client.Email;

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
        else if (client.Address is not null && Parametr.Address is not null)
        {
            client.Address.Street =
                Parametr.Address.Street ?? client.Address.Street;

            client.Address.Number =
                Parametr.Address.Number ?? client.Address.Number;

            client.Address.City =
                Parametr.Address.City ?? client.Address.City;

            client.Address.PostalCode =
                Parametr.Address.PostalCode ?? client.Address.PostalCode;

            client.Address.Country =
                Parametr.Address.Country ?? client.Address.Country;
        }

        var updatedClient = await clientRepository.UpdateAsync(
            client,
            cancellationToken);

        return ClientMappers.ToUpdateDto(updatedClient);
    }
}
