namespace CreateInvoiceSystem.Abstractions.DbContext;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public interface IDbContext : ISaveChangesContext
{
    DbSet<T> Set<T>() where T : class;
    ChangeTracker ChangeTracker { get; }
}
