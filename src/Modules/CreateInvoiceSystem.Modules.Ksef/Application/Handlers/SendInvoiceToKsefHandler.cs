using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.SendInvoiceToKsef;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Ksef.Application.Handlers;

public sealed class SendInvoiceToKsefHandler(IKsefOutboxRepository outboxRepository)
    : IRequestHandler<SendInvoiceToKsefRequest, int>
{
    public Task<int> Handle(SendInvoiceToKsefRequest request, CancellationToken cancellationToken) =>
        outboxRepository.EnqueueAsync(request.UserId, request.InvoiceId, cancellationToken);
}
