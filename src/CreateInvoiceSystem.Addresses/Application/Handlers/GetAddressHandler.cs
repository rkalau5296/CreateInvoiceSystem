namespace CreateInvoiceSystem.Addresses.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.Mappers;
using CreateInvoiceSystem.Addresses.Application.Queries;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.GetAddress;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class GetAddressHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetAddressRequest, GetAddressResponse>
{
    public async Task<GetAddressResponse> Handle(GetAddressRequest request, CancellationToken cancellationToken)
    {
        GetAddressQuery query = new(request.Id);
        var address = await queryExecutor.Execute(query);      

        return new GetAddressResponse
        {
            Data = AddressMappers.ToDto(address),
        };
    }
}
