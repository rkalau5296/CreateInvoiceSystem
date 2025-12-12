using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.InvoicePositions.Entities;
using CreateInvoiceSystem.Modules.Products.Services;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.InvoicePositions.Services;

public sealed class InvoicePositionReadService(IDbContext db) : IInvoicePositionReadService
{
    private readonly IDbContext _db = db;

    public Task<bool> IsProductUsedAsync(int productId, CancellationToken cancellationToken = default) =>
        _db.Set<InvoicePosition>()
           .AsNoTracking()
           .AnyAsync(ip => ip.ProductId == productId, cancellationToken);
}
