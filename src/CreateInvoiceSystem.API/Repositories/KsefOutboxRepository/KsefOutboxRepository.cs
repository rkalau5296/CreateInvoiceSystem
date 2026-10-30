using CreateInvoiceSystem.Modules.Ksef.Dto;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using CreateInvoiceSystem.Modules.Ksef.Persistence.Entities;
using CreateInvoiceSystem.Modules.Ksef.Persistence.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.API.Repositories.KsefOutboxRepository;

public class KsefOutboxRepository(IKsefDbContext db) : IKsefOutboxRepository
{
    public async Task<int> EnqueueAsync(int userId, int invoiceId, CancellationToken cancellationToken)
    {
        var entity = new KsefOutgoingInvoiceStatus
        {
            UserId = userId,
            InvoiceId = invoiceId,
            Status = KsefInvoiceStatus.Wysylanie,
            SentAt = DateTime.UtcNow
        };

        db.KsefOutgoingInvoiceStatuses.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return entity.KsefOutgoingInvoiceStatusId;
    }

    public async Task<List<KsefOutboxItem>> GetPendingAsync(CancellationToken cancellationToken)
    {
        return await db.KsefOutgoingInvoiceStatuses
            .Where(s => s.Status == KsefInvoiceStatus.Wysylanie)
            .Select(s => new KsefOutboxItem(s.KsefOutgoingInvoiceStatusId, s.UserId, s.InvoiceId))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkInProgressAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await db.KsefOutgoingInvoiceStatuses.SingleAsync(s => s.KsefOutgoingInvoiceStatusId == id, cancellationToken);
        entity.Status = KsefInvoiceStatus.OczekujeNaPotwierdzenie;        
    }

    public async Task MarkSucceededAsync(int id, KsefSendResult result, CancellationToken cancellationToken)
    {
        var entity = await db.KsefOutgoingInvoiceStatuses.SingleAsync(s => s.KsefOutgoingInvoiceStatusId == id, cancellationToken);
        entity.Status = KsefInvoiceStatus.Zaakceptowana;
        entity.ElementReferenceNumber = result.InvoiceReferenceNumber;
        entity.KsefReferenceNumber = result.KsefNumber;
        entity.UpoXml = result.UpoXml;
        entity.ConfirmedAt = DateTime.UtcNow;        
    }

    public async Task MarkFailedAsync(int id, string errorMessage, CancellationToken cancellationToken)
    {
        var entity = await db.KsefOutgoingInvoiceStatuses.SingleAsync(s => s.KsefOutgoingInvoiceStatusId == id, cancellationToken);
        entity.Status = KsefInvoiceStatus.Odrzucona;
        entity.ErrorMessage = errorMessage;        
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}