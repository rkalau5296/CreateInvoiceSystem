using MediatR;

namespace CreateInvoiceSystem.Abstractions.CQRS;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
