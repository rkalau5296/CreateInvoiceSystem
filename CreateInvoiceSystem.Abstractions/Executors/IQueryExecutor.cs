namespace CreateInvoiceSystem.Abstractions.Executors;

using CreateInvoiceSystem.Abstractions.CQRS;

public interface IQueryExecutor
{
    Task<TResult> Execute<TResult>(QueryBase<TResult> query);
}
