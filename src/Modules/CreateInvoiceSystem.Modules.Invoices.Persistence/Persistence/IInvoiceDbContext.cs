using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Invoices.Persistence.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Invoices.Persistence.Persistence;

public interface IInvoiceDbContext : ISaveChangesContext
{
    DbSet<InvoiceEntity> Invoices { get; set; }        
}
