using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Clients.Persistence.Persistence;
public interface IClientDbContext : ISaveChangesContext
{
    DbSet<ClientEntity> Clients { get; set; }
}