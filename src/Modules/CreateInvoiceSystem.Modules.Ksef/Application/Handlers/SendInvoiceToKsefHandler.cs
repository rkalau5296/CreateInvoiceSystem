using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.SendInvoiceToKsef;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Ksef.Application.Handlers;

public sealed class SendInvoiceToKsefHandler(IKsefOutboxRepository outboxRepository, IKsefCredentialsStore credentialsStore)
    : IRequestHandler<SendInvoiceToKsefRequest, int>
{
    public async Task<int> Handle(SendInvoiceToKsefRequest request, CancellationToken cancellationToken)
    {
        await credentialsStore.GetRequiredAsync(request.UserId, cancellationToken);
        return await outboxRepository.EnqueueAsync(request.UserId, request.InvoiceId, cancellationToken);
    }
}
