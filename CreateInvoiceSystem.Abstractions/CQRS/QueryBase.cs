using CreateInvoiceSystem.Abstractions.DbContext;

namespace CreateInvoiceSystem.Abstractions.CQRS;

public abstract class QueryBase<TResult>
{
    public abstract Task<TResult> Execute(IDbContext context, CancellationToken cancellationToken=default);
}
