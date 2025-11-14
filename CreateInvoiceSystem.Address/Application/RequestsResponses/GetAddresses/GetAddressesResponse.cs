namespace CreateInvoiceSystem.Address.Application.RequestsResponses.GetAddresses;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Address.Application.DTO;

public class GetAddressesResponse : ResponseBase<List<AddressDto>>
{
}