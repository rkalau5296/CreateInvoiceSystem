namespace CreateInvoiceSystem.Abstractions.CQRS;

using CreateInvoiceSystem.Abstractions.DbContext;

public abstract class CommandBase<TParametr, TResult>
{
    public TParametr? Parametr { get; set; }

    public abstract Task<TResult> Execute(IDbContext context, CancellationToken cancellationToken = default);
}
