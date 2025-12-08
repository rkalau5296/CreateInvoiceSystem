using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Clients.Application.Queries;
using CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.GetClient;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Application.Handlers;

public class GetClientHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetClientRequest, GetClientResponse>
{
    public async Task<GetClientResponse> Handle(GetClientRequest request, CancellationToken cancellationToken)
    {
        GetClientQuery query = new(request.Id);
        var client = await queryExecutor.Execute(query);      

        return new GetClientResponse
        {
            Data = ClientMappers.ToDto(client),
        };
    }
}
