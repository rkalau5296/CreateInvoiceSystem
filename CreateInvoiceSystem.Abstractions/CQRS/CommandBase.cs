namespace CreateInvoiceSystem.Abstractions.CQRS;

using CreateInvoiceSystem.Abstractions.DbContext;

public abstract class CommandBase<TParametr, TResult>
{
    public TParametr? Parametr { get; set; }

    public abstract Task<TResult> Execute(ICreateInvoiceSystemDbContext context, CancellationToken cancellationToken = default);
}
