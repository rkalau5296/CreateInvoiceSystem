using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Invoices.Persistence.Persistence;

public interface IInvoiceDbContext : ISaveChangesContext
{
    DbSet<InvoiceEntity> Invoices { get; set; }        
}
