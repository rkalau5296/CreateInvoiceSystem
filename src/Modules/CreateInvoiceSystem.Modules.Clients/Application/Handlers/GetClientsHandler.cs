using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Clients.Application.Queries;
using CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Modules.Clients.Entities;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Application.Handlers;

public class GetClientsHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetClientsRequest, GetClientsResponse>
{
    public async Task<GetClientsResponse> Handle(GetClientsRequest request, CancellationToken cancellationToken)
    {
        GetClientsQuery query = new();

        List<Client> clients = await queryExecutor.Execute(query);

        return new GetClientsResponse
        {
            Data = ClientMappers.ToDtoList(clients)
        }; 
    }
}