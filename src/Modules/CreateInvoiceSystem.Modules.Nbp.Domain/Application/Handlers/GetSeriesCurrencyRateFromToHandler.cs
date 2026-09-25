using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRate;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
public class GetSeriesCurrencyRateFromToHandler(IOptions<NbpApiOptions> options, INbpApiRestService _nbpApiRestService) : IRequestHandler<GetSeriesCurrencyRateFromToRequest, GetSeriesCurrencyRateFromToResponse>
{
    public async Task<GetSeriesCurrencyRateFromToResponse> Handle(GetSeriesCurrencyRateFromToRequest request, CancellationToken cancellationToken)
    {
        var rate = await _nbpApiRestService.GetSeriesCurrencyRateFromToAsync(
            options.Value.BaseUrl, request.TableName, request.CurrencyCode, request.DateFrom, request.DateTo, cancellationToken);     

        return new GetSeriesCurrencyRateFromToResponse
        {
            Data = rate
        };
    }
}
