using CreateInvoiceSystem.Abstractions.CQRS;
using MediatR;

namespace CreateInvoiceSystem.Addresses.Application.RequestsResponses.GetAddress;

public record GetAddressRequest(int Id) : IRequest<GetAddressResponse>
{
    public int Id { get; set; } = Id;
}
