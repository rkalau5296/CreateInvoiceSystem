using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
public class GetClientsHandler(IQueryExecutor queryExecutor, IClientRepository _clientRepository) : IRequestHandler<GetClientsRequest, GetClientsResponse>
{
    public async Task<GetClientsResponse> Handle(GetClientsRequest request, CancellationToken cancellationToken)
    {
        GetClientsQuery query = new();

        List<Client> clients = await queryExecutor.Execute(query, _clientRepository, cancellationToken);

        return new GetClientsResponse
        {
            Data = clients.ToDtoList()
        }; 
    }
}