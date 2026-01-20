using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Persistence;

public interface IInvoicePosistionDbContext : ISaveChangesContext
{
    DbSet<InvoicePositionEntity> InvoicePositions { get; set; }
}
