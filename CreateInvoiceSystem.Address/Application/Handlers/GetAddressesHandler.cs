namespace CreateInvoiceSystem.Address.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Address.Domain.Entities;
using CreateInvoiceSystem.Address.Application.Queries;
using CreateInvoiceSystem.Address.Application.RequestsResponses.GetAddresses;
using MediatR;
using CreateInvoiceSystem.Address.Application.DTO;
using CreateInvoiceSystem.Address.Application.Mappers;

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