using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
public class CreateClientCommand : CommandBase<CreateClientDto, ClientDto, IClientRepository>
{    
    public override async Task<ClientDto> Execute(IClientRepository _clientRepository, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(_clientRepository));
        if (this.Parametr.Address is null)
            throw new ArgumentNullException(nameof(this.Parametr.Address));

        var exists = await _clientRepository.ExistsAsync(
            this.Parametr.Name,
            this.Parametr.Address.Street,
            this.Parametr.Address.Number,
            this.Parametr.Address.City,
            this.Parametr.Address.PostalCode,
            this.Parametr.Address.Country,
            this.Parametr.UserId,
            cancellationToken);

        if (exists)
            throw new InvalidOperationException("A client with the same name and address already exists.");

        var domainModel = ClientMappers.ToEntity(this.Parametr);

        var savedClient = await _clientRepository.AddAsync(domainModel, cancellationToken);
        await _clientRepository.SaveChangesAsync(cancellationToken);

        var persisted = await _clientRepository.GetByIdAsync(savedClient.ClientId, savedClient.UserId, cancellationToken);

        return persisted is not null
            ? savedClient.ToDto()
            : throw new InvalidOperationException("Client was saved but could not be reloaded.");
    }
}          