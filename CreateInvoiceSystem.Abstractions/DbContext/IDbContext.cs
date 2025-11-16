using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Abstractions.DbContext;

public interface IDbContext
{
    DbSet<T> Set<T>() where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
