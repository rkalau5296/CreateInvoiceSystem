using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
public class CreateClientCommand : CommandBase<CreateClientDto, ClientDto, IClientRepository>
{
    public override async Task<ClientDto> Execute(IClientRepository clientRepository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Parametr);
        ArgumentNullException.ThrowIfNull(Parametr.Address, "Address");

        var exists = await clientRepository.ExistsAsync(
            Parametr.Name,
            Parametr.Address.Street,
            Parametr.Address.Number,
            Parametr.Address.City,
            Parametr.Address.PostalCode,
            Parametr.Address.Country,
            Parametr.UserId,
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "Istnieje już taki klient z identycznymi danymi.");
        }

        var domainModel = ClientMappers.ToEntity(Parametr);
        domainModel.UserId = Parametr.UserId;

        var savedClient = await clientRepository.AddAsync(
            domainModel,
            cancellationToken);

        return savedClient.ToDto();
    }
}          