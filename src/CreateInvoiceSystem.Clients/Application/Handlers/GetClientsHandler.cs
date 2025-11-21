namespace CreateInvoiceSystem.Clients.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Clients.Domain.Entities;
using CreateInvoiceSystem.Clients.Application.Queries;
using CreateInvoiceSystem.Clients.Application.RequestsResponses.GetClients;
using MediatR;
using CreateInvoiceSystem.Clients.Application.Mappers;

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