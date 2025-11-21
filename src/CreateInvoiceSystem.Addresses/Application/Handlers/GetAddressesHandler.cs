namespace CreateInvoiceSystem.Addresses.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Addresses.Domain.Entities;
using CreateInvoiceSystem.Addresses.Application.Queries;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.GetAddresses;
using MediatR;
using CreateInvoiceSystem.Addresses.Application.Mappers;

public class GetAddressesHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetAddressesRequest, GetAddressesResponse>
{
    public async Task<GetAddressesResponse> Handle(GetAddressesRequest request, CancellationToken cancellationToken)
    {
        GetAddressesQuery query = new();

        List<Address> addresses = await queryExecutor.Execute(query);

        return new GetAddressesResponse
        {
            Data = AddressMappers.ToDtoList(addresses)
        }; 
    }
}