using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using System.Threading;

namespace CreateInvoiceSystem.Abstractions.Executors;

public class CommandExecutor(ICreateInvoiceSystemDbContext context) : ICommandExecutor
{
    private readonly ICreateInvoiceSystemDbContext context = context;

    public Task<TResult> Execute<TParameters, TResult>(CommandBase<TParameters, TResult> command, CancellationToken cancellationToken = default)
    {
        return command.Execute(context, cancellationToken);
    }
}
