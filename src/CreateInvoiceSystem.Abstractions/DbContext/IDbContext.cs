namespace CreateInvoiceSystem.Abstractions.DbContext;

using Microsoft.EntityFrameworkCore;

public interface IDbContext : ISaveChangesContext
{
    DbSet<T> Set<T>() where T : class;
}
