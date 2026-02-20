using CreateInvoiceSystem.Persistence;
using MediatR;

namespace CreateInvoiceSystem.API.TransactionBehavior;

public class TransactionBehavior<TRequest, TResponse>(
    CreateInvoiceSystemDbContext dbContext,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!request.GetType().Name.EndsWith("Command"))
        {
            return await next();
        }

        using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        try
        {
            var response = await next();

            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Błąd w {CommandName}. Cofnięto zmiany w bazie danych.", request.GetType().Name);
            throw;
        }
    }
}