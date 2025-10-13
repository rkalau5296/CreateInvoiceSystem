using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;

namespace CreateInvoiceSystem.Abstractions.Executors;

public class CommandExecutor(ICreateInvoiceSystemDbContext context) : ICommandExecutor
{
    private readonly ICreateInvoiceSystemDbContext context = context;

    public Task<TResult> Execute<TParameters, TResult>(CommandBase<TParameters, TResult> command)
    {
        return command.Execute(context);
    }
}
