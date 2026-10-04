using CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Modules.Clients.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Application.Handlers;
public class GetClientsHandler(IClientRepository _clientRepository) : IRequestHandler<GetClientsRequest, GetClientsResponse>
{
    public async Task<GetClientsResponse> Handle(GetClientsRequest request, CancellationToken cancellationToken)
    {
        var clients = await _clientRepository.GetAllAsync(request.UserId, request.PageNumber, request.PageSize, request.SearchTerm, cancellationToken) 
                ?? throw new InvalidOperationException("List of clients is empty.");
        
        return new GetClientsResponse
        {
            Data = clients.Items.ToDtoList(),
            TotalCount = clients.TotalCount
        };
    }
}