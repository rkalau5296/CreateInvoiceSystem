using CreateInvoiceSystem.Abstractions.CQRS;

namespace CreateInvoiceSystem.Abstractions.Executors;

public class QueryExecutor : IQueryExecutor
{
    public Task<TResult> Execute<TResult, TDependency>(
        QueryBase<TResult, TDependency> query,
        TDependency dependency,
        CancellationToken cancellationToken = default)
    {
        return query.Execute(dependency, cancellationToken);
    }
}
