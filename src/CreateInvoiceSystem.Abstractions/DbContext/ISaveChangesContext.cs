namespace CreateInvoiceSystem.Abstractions.DbContext;

public interface ISaveChangesContext
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
