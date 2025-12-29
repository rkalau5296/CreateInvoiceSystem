namespace CreateInvoiceSystem.Abstractions.CQRS;

public abstract class QueryBase<TResult, TDependency>
{
    public abstract Task<TResult> Execute(TDependency dependency, CancellationToken cancellationToken=default);
}
