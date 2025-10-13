using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;

namespace CreateInvoiceSystem.Abstractions.Executors;

public class QueryExecutor : IQueryExecutor
{
    private readonly ICreateInvoiceSystemDbContext invoiceContext;

    public QueryExecutor(ICreateInvoiceSystemDbContext invoiceContext)
    {
        this.invoiceContext = invoiceContext;
    }
    public Task<TResult> Execute<TResult>(QueryBase<TResult> query)
    {
        return query.Execute(invoiceContext);
    }
}
