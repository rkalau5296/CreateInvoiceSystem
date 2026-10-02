using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.DeleteClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
public class DeleteClientHandler(IClientRepository _clientRepository) : IRequestHandler<DeleteClientRequest, DeleteClientResponse>
{
    public async Task<DeleteClientResponse> Handle(DeleteClientRequest request, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetByIdAsync(request.Id, request.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"Client with ID {request.Id} not found.");

        var clientDto = ClientMappers.ToDto(client);

        await _clientRepository.RemoveAsync(client.ClientId, cancellationToken);

        if (client.Address is not null)
        {
            await _clientRepository.RemoveAddressAsync(client.AddressId, cancellationToken);
        }

        return new DeleteClientResponse()
        {
            Data = clientDto
        };
    }
}