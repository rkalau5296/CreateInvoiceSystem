using CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.PreviousDatesRate;
using CreateInvoiceSystem.Modules.Nbp.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Nbp.Application.Handlers;
public class GetSeriesCurrencyRateFromToHandler(INbpApiRestService _nbpApiRestService) : IRequestHandler<GetSeriesCurrencyRateFromToRequest, GetSeriesCurrencyRateFromToResponse>
{
    public async Task<GetSeriesCurrencyRateFromToResponse> Handle(GetSeriesCurrencyRateFromToRequest request, CancellationToken cancellationToken)
    {
        var rate = await _nbpApiRestService.GetSeriesCurrencyRateFromToAsync(
           request.TableName, request.CurrencyCode, request.DateFrom, request.DateTo, cancellationToken);     

        return new GetSeriesCurrencyRateFromToResponse
        {
            Data = rate
        };
    }
}