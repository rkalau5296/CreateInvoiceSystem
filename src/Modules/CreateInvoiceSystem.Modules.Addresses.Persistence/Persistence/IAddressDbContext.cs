using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Shared.Persistence;
using Microsoft.EntityFrameworkCore;


namespace CreateInvoiceSystem.Modules.Addresses.Persistence.Persistence;
public interface IAddressDbContext : ISaveChangesContext
{
    DbSet<AddressEntity> Addresses { get; set; }
}
