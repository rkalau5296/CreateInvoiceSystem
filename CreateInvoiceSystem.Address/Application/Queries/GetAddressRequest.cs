using CreateInvoiceSystem.Abstractions.CQRS;
using MediatR;

namespace CreateInvoiceSystem.Address.Application.Queries;

public record GetAddressRequest(int id) : IRequest<GetAddressResponse>
{
    public int Id { get; set; } = id;
}
