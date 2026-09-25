using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.CreateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
public class CreateClientHandler(IClientRepository _clientRepository) : IRequestHandler<CreateClientRequest, CreateClientResponse>
{
    public async Task<CreateClientResponse> Handle(CreateClientRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Client);
        ArgumentNullException.ThrowIfNull(request.Client.Address, "Address");

        var exists = await _clientRepository.ExistsAsync(
            request.Client.Name,
            request.Client.Address.Street,
            request.Client.Address.Number,
            request.Client.Address.City,
            request.Client.Address.PostalCode,
            request.Client.Address.Country,
            request.Client.UserId,
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "Istnieje już taki klient z identycznymi danymi.");
        }

        var domainModel = ClientMappers.ToEntity(request.Client);
        domainModel.UserId = request.Client.UserId;

        var savedClient = await _clientRepository.AddAsync(
            domainModel,
            cancellationToken);        

        return new CreateClientResponse()
        {
            Data = savedClient.ToDto()
        };
    }
}