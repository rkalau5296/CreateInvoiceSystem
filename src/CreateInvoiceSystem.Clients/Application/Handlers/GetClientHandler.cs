namespace CreateInvoiceSystem.Clients.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.Mappers;
using CreateInvoiceSystem.Clients.Application.Queries;
using CreateInvoiceSystem.Clients.Application.RequestsResponses.GetClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

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
