using CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.ActualRate;
using CreateInvoiceSystem.Modules.Nbp.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Nbp.Application.Handlers;
public class GetActualCurrencyRateHandler(INbpApiRestService _nbpApiRestService) : IRequestHandler<GetActualCurrencyRateRequest, GetActualCurrencyRateResponse>
{
    public async Task<GetActualCurrencyRateResponse> Handle(GetActualCurrencyRateRequest request, CancellationToken cancellationToken)
    {
        var rates = await _nbpApiRestService.GetActualCurrencyRateAsync(
            request.TableName, request.CurrencyCode, cancellationToken);        
        
        return new GetActualCurrencyRateResponse
        {
            Data = rates
        };
    }
}
