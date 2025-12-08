using CreateInvoiceSystem.Modules.Clients.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Clients.Persistence;

public interface IClientDbContext : IDbContext
{
    public DbSet<Client> Clients { get; set; }
}

public interface IDbContext
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
