namespace CreateInvoiceSystem.Abstractions.CQRS;

using MediatR;

public interface IQueryHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
where TRequest : IQuery<TResponse>
{
}
