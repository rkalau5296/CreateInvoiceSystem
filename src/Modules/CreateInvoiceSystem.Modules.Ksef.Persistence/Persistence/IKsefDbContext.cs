using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Ksef.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Ksef.Persistence.Persistence;

public interface IKsefDbContext : ISaveChangesContext
{
    DbSet<KsefOutgoingInvoiceStatus> KsefOutgoingInvoiceStatuses { get; set; }
}