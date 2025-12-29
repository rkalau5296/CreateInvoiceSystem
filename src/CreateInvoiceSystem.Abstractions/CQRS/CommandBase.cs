namespace CreateInvoiceSystem.Abstractions.CQRS;

public abstract class CommandBase<TParametr, TResult, TDependency>
{
    public TParametr Parametr { get; set; }

    public abstract Task<TResult> Execute(TDependency dependency, CancellationToken cancellationToken = default);
}
