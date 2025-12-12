using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Clients.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Clients.Persistence;

public interface IClientDbContext : ISaveChangesContext
{
    public DbSet<Client> Clients { get; set; }
}