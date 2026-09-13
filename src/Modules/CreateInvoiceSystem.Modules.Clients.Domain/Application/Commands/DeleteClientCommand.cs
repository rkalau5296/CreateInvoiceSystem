using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;

public class DeleteClientCommand : CommandBase<Client, ClientDto, IClientRepository>
{
    public override async Task<ClientDto> Execute(IClientRepository clientRepository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Parametr);

        var client = await clientRepository.GetByIdAsync(Parametr.ClientId, Parametr.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"Client with ID {Parametr.ClientId} not found.");

        var clientDto = ClientMappers.ToDto(client);

        await clientRepository.RemoveAsync(client.ClientId, cancellationToken);

        if (client.Address is not null)
        {
            await clientRepository.RemoveAddressAsync(client.AddressId, cancellationToken);
        }

        return clientDto;
    }
}
