using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
public class GetClientHandler(IQueryExecutor queryExecutor, IClientRepository _clientRepository) : IRequestHandler<GetClientRequest, GetClientResponse>
{
    public async Task<GetClientResponse> Handle(GetClientRequest request, CancellationToken cancellationToken)
    {
        GetClientQuery query = new(request.Id, request.UserId);
        var client = await queryExecutor.Execute(query, _clientRepository, cancellationToken);

        return new GetClientResponse
        {
            Data = client.ToDto(),
        };
    }
}