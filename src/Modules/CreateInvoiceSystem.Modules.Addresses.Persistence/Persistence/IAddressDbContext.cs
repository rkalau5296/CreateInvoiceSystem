using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using Microsoft.EntityFrameworkCore;


namespace CreateInvoiceSystem.Modules.Addresses.Persistence.Persistence;
public interface IAddressDbContext : ISaveChangesContext
{
    DbSet<AddressEntity> Addresses { get; set; }
}
