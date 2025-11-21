using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;

namespace CreateInvoiceSystem.Abstractions.Executors;

public class CommandExecutor(IDbContext context) : ICommandExecutor
{
    private readonly IDbContext context = context;

    public Task<TResult> Execute<TParameters, TResult>(CommandBase<TParameters, TResult> command, CancellationToken cancellationToken = default)
    {
        return command.Execute(context, cancellationToken);
    }
}
