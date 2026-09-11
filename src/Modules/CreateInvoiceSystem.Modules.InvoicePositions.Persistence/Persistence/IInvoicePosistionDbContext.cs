using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Invoices.Persistence.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Persistence;

public interface IInvoicePosistionDbContext : ISaveChangesContext
{
    DbSet<InvoicePositionEntity> InvoicePositions { get; set; }
}
