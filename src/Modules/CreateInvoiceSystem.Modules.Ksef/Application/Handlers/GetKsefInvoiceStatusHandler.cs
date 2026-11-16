using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceStatus;
using CreateInvoiceSystem.Modules.Ksef.Dto;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Ksef.Application.Handlers;

public sealed class GetKsefInvoiceStatusHandler(IKsefOutboxRepository outboxRepository)
    : IRequestHandler<GetKsefInvoiceStatusRequest, KsefOutboxStatus?>
{
    public Task<KsefOutboxStatus?> Handle(GetKsefInvoiceStatusRequest request, CancellationToken cancellationToken) =>
        outboxRepository.GetStatusByInvoiceIdAsync(request.UserId, request.InvoiceId, cancellationToken);
}
