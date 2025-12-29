using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
public class CreateClientCommand : CommandBase<CreateClientDto, CreateClientDto, IClientRepository>
{    
    public override async Task<CreateClientDto> Execute(IClientRepository _clientRepository, CancellationToken cancellationToken = default)
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

        var entity = ClientMappers.ToEntity(this.Parametr);

        await _clientRepository.AddAsync(entity, cancellationToken);
        await _clientRepository.SaveChangesAsync(cancellationToken);

        var persisted = await _clientRepository.GetByIdAsync(entity.ClientId, true, cancellationToken);

        return persisted is not null
            ? entity.ToCreateDto()
            : throw new InvalidOperationException("Client was saved but could not be reloaded.");
    }
}          