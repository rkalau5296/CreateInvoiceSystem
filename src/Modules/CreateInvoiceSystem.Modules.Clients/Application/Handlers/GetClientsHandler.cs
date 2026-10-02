using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
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