namespace CreateInvoiceSystem.Address.Application.Queries;

using AddressEntity = Domain.Entities.Address;
using CreateInvoiceSystem.Abstractions.CQRS;

public class GetAddressResponse: ResponseBase<AddressEntity>
{
}
