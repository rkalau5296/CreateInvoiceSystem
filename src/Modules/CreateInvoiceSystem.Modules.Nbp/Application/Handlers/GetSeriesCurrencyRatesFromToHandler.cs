using CreateInvoiceSystem.Modules.Nbp.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.PreviousDatesRates;
using CreateInvoiceSystem.Modules.Nbp.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Nbp.Application.Handlers;
public class GetSeriesCurrencyRatesFromToHandler(INbpApiRestService _nbpApiRestService) : IRequestHandler<GetSeriesCurrencyRatesFromToRequest, GetSeriesCurrencyRatesFromToResponse>
{
    public async Task<GetSeriesCurrencyRatesFromToResponse> Handle(GetSeriesCurrencyRatesFromToRequest request, CancellationToken cancellationToken)
    {
        var rates = await _nbpApiRestService.GetSeriesCurrencyRatesFromToAsync(
            request.TableName, request.DateFrom, request.DateTo, cancellationToken);
        
        return new GetSeriesCurrencyRatesFromToResponse
        {
            Data = rates
        };
    }
}
