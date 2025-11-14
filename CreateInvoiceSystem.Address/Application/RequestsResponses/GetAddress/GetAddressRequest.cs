using CreateInvoiceSystem.Abstractions.CQRS;
using MediatR;

namespace CreateInvoiceSystem.Address.Application.RequestsResponses.GetAddress;

public record GetAddressRequest(int Id) : IRequest<GetAddressResponse>
{
    public int Id { get; set; } = Id;
}
