namespace CreateInvoiceSystem.Address.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Address.Application.Mappers;
using CreateInvoiceSystem.Address.Application.Queries;
using CreateInvoiceSystem.Address.Application.RequestsResponses.GetAddress;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class GetAddressHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetAddressRequest, GetAddressResponse>
{
    private readonly IQueryExecutor _queryExecutor = queryExecutor;

    public async Task<GetAddressResponse> Handle(GetAddressRequest request, CancellationToken cancellationToken)
    {
        GetAddressQuery query = new(request.Id);
        var address = await _queryExecutor.Execute(query);                

        GetAddressResponse response = new()
        {
            Data = AddressMappers.ToDto(address),
        };

        return response;
    }
}
