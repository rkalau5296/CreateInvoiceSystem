namespace CreateInvoiceSystem.Address.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Address.Application.Queries;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class GetAddressHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetAddressRequest, GetAddressResponse>
{
    private readonly IQueryExecutor queryExecutor = queryExecutor;

    public async Task<GetAddressResponse> Handle(GetAddressRequest request, CancellationToken cancellationToken)
    {
        GetAddressQuery query = new(request.Id);
        var address = await queryExecutor.Execute(query);

        //TODO: Automapper or Mapper

        GetAddressResponse response = new()
        {
            Data = address, //address mapped to response
        };

        return response;
    }
}
