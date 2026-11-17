using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceDetails;
using CreateInvoiceSystem.Modules.Ksef.Dto;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using CreateInvoiceSystem.Modules.Ksef.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Ksef.Application.Handlers;

public sealed class GetKsefInvoiceDetailsHandler(IKsefApiClient apiClient)
    : IRequestHandler<GetKsefInvoiceDetailsRequest, KsefInvoiceDetailsDto>
{
    public async Task<KsefInvoiceDetailsDto> Handle(
        GetKsefInvoiceDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var xml = await apiClient.DownloadInvoiceXmlAsync(request.UserId, request.KsefNumber, cancellationToken);
        return KsefInvoiceXmlMapper.ToDetailsDto(request.KsefNumber, xml);
    }
}
