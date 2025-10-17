using CreateInvoiceSystem.Abstractions.CQRS;

namespace CreateInvoiceSystem.Abstractions.Executors;

public interface ICommandExecutor
{
    Task<TResult> Execute<TParameters, TResult>(CommandBase<TParameters, TResult> command);
}
