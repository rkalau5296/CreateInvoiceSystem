using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;

namespace CreateInvoiceSystem.Abstractions.Executors;

public class QueryExecutor(ICreateInvoiceSystemDbContext invoiceContext) : IQueryExecutor
{
    private readonly ICreateInvoiceSystemDbContext invoiceContext = invoiceContext;

    public Task<TResult> Execute<TResult>(QueryBase<TResult> query)
    {
        return query.Execute(invoiceContext);
    }
}
