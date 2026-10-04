using CreateInvoiceSystem.Modules.Nbp.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.ActualRate;
using CreateInvoiceSystem.Modules.Nbp.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace CreateInvoiceSystem.Modules.Nbp.Application.Handlers;
public class GetActualCurrencyRateHandler(IOptions<NbpApiOptions> options, INbpApiRestService _nbpApiRestService) : IRequestHandler<GetActualCurrencyRateRequest, GetActualCurrencyRateResponse>
{
    public async Task<GetActualCurrencyRateResponse> Handle(GetActualCurrencyRateRequest request, CancellationToken cancellationToken)
    {
        var rates = await _nbpApiRestService.GetActualCurrencyRateAsync(
            options.Value.BaseUrl, request.TableName, request.CurrencyCode, cancellationToken);        
        
        return new GetActualCurrencyRateResponse
        {
            Data = rates
        };
    }
}
