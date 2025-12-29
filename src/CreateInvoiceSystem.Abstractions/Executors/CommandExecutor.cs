using CreateInvoiceSystem.Abstractions.CQRS;

namespace CreateInvoiceSystem.Abstractions.Executors;

public class CommandExecutor : ICommandExecutor
{
    public Task<TResult> Execute<TParam, TResult, TDependency>(
        CommandBase<TParam, TResult, TDependency> command,
        TDependency dependency,
        CancellationToken cancellationToken = default)
    {
        return command.Execute(dependency, cancellationToken);
    }
}