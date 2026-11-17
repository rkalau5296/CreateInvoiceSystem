using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetIncomingKsefInvoices;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Ksef.Application.Handlers;

public sealed class GetIncomingKsefInvoicesHandler(IKsefApiClient apiClient)
    : IRequestHandler<GetIncomingKsefInvoicesRequest, GetIncomingKsefInvoicesResponse>
{
    public async Task<GetIncomingKsefInvoicesResponse> Handle(
        GetIncomingKsefInvoicesRequest request,
        CancellationToken cancellationToken)
    {
        var page = await apiClient.GetIncomingInvoicesAsync(
            request.UserId,
            request.FromDate,
            request.ToDate,
            (request.PageNumber - 1) * request.PageSize,
            request.PageSize,
            cancellationToken);

        return new GetIncomingKsefInvoicesResponse
        {
            Data = page.Invoices.ToList(),
            HasMore = page.HasMore
        };
    }
}
