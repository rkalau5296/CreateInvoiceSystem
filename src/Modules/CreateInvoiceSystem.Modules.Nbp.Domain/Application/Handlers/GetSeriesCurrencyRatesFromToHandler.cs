using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRates;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
public class GetSeriesCurrencyRatesFromToHandler(IOptions<NbpApiOptions> options, INbpApiRestService _nbpApiRestService) : IRequestHandler<GetSeriesCurrencyRatesFromToRequest, GetSeriesCurrencyRatesFromToResponse>
{
    public async Task<GetSeriesCurrencyRatesFromToResponse> Handle(GetSeriesCurrencyRatesFromToRequest request, CancellationToken cancellationToken)
    {
        var rates = await _nbpApiRestService.GetSeriesCurrencyRatesFromToAsync(
            options.Value.BaseUrl, request.TableName, request.DateFrom, request.DateTo, cancellationToken);
        
        return new GetSeriesCurrencyRatesFromToResponse
        {
            Data = rates
        };
    }
}
