using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceXml;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Ksef.Application.Handlers;

public sealed class GetKsefInvoiceXmlHandler(IKsefApiClient apiClient)
    : IRequestHandler<GetKsefInvoiceXmlRequest, string>
{
    public Task<string> Handle(GetKsefInvoiceXmlRequest request, CancellationToken cancellationToken) =>
        apiClient.DownloadInvoiceXmlAsync(request.UserId, request.KsefNumber, cancellationToken);
}
