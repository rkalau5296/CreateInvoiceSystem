namespace CreateInvoiceSystem.Abstractions.Executors;

using CreateInvoiceSystem.Abstractions.CQRS;

public interface IQueryExecutor
{    
    Task<TResult> Execute<TResult, TDependency>(
        QueryBase<TResult, TDependency> query,
        TDependency dependency,
        CancellationToken cancellationToken = default);
}
