using CreateInvoiceSystem.Modules.Ksef.Dto;

namespace CreateInvoiceSystem.Modules.Ksef.Interfaces;

public interface IKsefOutboxRepository
{
    Task<int> EnqueueAsync(int userId, int invoiceId, CancellationToken cancellationToken);
    Task<List<KsefOutboxItem>> GetPendingAsync(CancellationToken cancellationToken);
    Task MarkInProgressAsync(int id, CancellationToken cancellationToken);
    Task MarkSucceededAsync(int id, KsefSendResult result, CancellationToken cancellationToken);
    Task MarkFailedAsync(int id, string errorMessage, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}