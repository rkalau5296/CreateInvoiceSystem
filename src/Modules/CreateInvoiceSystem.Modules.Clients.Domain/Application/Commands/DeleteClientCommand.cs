using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
public class DeleteClientCommand : CommandBase<Client, ClientDto, IClientRepository>
{
    public override async Task<ClientDto> Execute(IClientRepository _clientRepository, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(Parametr));

        var clientEntity = await _clientRepository.GetByIdAsync(
            Parametr.ClientId,            
            cancellationToken) ?? throw new InvalidOperationException($"Client with ID {Parametr.ClientId} not found.");

        if (clientEntity.Address is not null)
            await _clientRepository.RemoveAddressAsync(clientEntity.Address, cancellationToken);

        await _clientRepository.RemoveAsync(clientEntity, cancellationToken);
        await _clientRepository.SaveChangesAsync(cancellationToken);

        var stillExists = await _clientRepository.ExistsByIdAsync(clientEntity.ClientId, cancellationToken);

        var addrExists = clientEntity.Address is not null && await _clientRepository.AddressExistsByIdAsync(clientEntity.Address.AddressId, cancellationToken);

        return (!stillExists && !addrExists)
            ? ClientMappers.ToDto(clientEntity)
            : throw new InvalidOperationException($"Failed to delete Client or Client address with ID {Parametr.ClientId}.");
    }
}
