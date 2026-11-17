using CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.GetClient;
using CreateInvoiceSystem.Modules.Clients.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Application.Handlers;
public class GetClientHandler(IClientRepository _clientRepository) : IRequestHandler<GetClientRequest, GetClientResponse>
{
    public async Task<GetClientResponse> Handle(GetClientRequest request, CancellationToken cancellationToken)
    {
        var client =  await _clientRepository.GetByIdAsync(request.Id, request.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"Client with ID {request.Id} not found or access denied.");

        return new GetClientResponse
        {
            Data = client.ToDto(),
        };
    }
}