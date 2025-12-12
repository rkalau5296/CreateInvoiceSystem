using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Clients.Services;
using CreateInvoiceSystem.Modules.Invoices.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Invoices.Services
{
    public sealed class InvoiceReadService(IDbContext db) : IInvoiceReadService
    {
        private readonly IDbContext _db = db;

        public Task<bool> IsClientUsedAsync(int clientId, CancellationToken cancellationToken = default) =>
            _db.Set<Invoice>()
               .AsNoTracking()
               .AnyAsync(ip => ip.ClientId == clientId, cancellationToken);
    }
}
