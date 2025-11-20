namespace CreateInvoiceSystem.Abstractions.DbContext;

using Microsoft.EntityFrameworkCore;

public interface IDbContext
{
    DbSet<T> Set<T>() where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
