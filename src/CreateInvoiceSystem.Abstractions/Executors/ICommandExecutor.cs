using CreateInvoiceSystem.Abstractions.CQRS;

namespace CreateInvoiceSystem.Abstractions.Executors;

public interface ICommandExecutor
{
    Task<TResult> Execute<TParam, TResult, TDependency>(CommandBase<TParam, TResult, TDependency> command,
       TDependency dependency, CancellationToken cancellationToken = default);
}
