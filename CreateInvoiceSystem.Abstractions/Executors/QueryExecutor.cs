using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;

namespace CreateInvoiceSystem.Abstractions.Executors;

public class QueryExecutor(IDbContext invoiceContext) : IQueryExecutor
{
    private readonly IDbContext invoiceContext = invoiceContext;

    public Task<TResult> Execute<TResult>(QueryBase<TResult> query)
    {
        return query.Execute(invoiceContext);
    }
}
