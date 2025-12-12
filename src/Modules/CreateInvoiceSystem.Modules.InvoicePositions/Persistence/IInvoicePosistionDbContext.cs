using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.InvoicePositions.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.InvoicePositions.Persistence;

public interface IInvoicePosistionDbContext : ISaveChangesContext
{
    public DbSet<InvoicePosition> InvoicePositions { get; set; }
}
