namespace CreateInvoiceSystem.Address.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Address.Application.DTO;
using CreateInvoiceSystem.Address.Application.Queries;
using CreateInvoiceSystem.Address.Application.RequestsResponses.GetAddresses;
using MediatR;

public class GetAddressesHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetAddressesRequest, GetAddressesResponse>
{
    private readonly IQueryExecutor queryExecutor = queryExecutor;    

    public async Task<GetAddressesResponse> Handle(GetAddressesRequest request, CancellationToken cancellationToken)
    {
        GetAddressesQuery query = new();

        List<AddressDto> addresses = await this.queryExecutor.Execute(query);

        GetAddressesResponse response = new()
        {
            Data = addresses
        };

        return response;
    }
}