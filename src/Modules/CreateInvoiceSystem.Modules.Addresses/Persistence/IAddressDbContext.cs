using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Entities;
using Microsoft.EntityFrameworkCore;


namespace CreateInvoiceSystem.Modules.Addresses.Persistence;

public interface IAddressDbContext : ISaveChangesContext
{
    public DbSet<Address> Addresses { get; set; }
}
