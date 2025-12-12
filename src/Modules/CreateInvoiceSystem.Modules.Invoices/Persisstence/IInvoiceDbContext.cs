using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.InvoicePositions.Entities;
using CreateInvoiceSystem.Modules.Invoices.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Invoices.Persisstence;

public interface IInvoiceDbContext : ISaveChangesContext
{
    public DbSet<Invoice> Invoices { get; set; }        
}
