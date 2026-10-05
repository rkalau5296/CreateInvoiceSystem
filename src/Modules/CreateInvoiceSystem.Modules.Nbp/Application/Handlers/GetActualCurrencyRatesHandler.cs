using CreateInvoiceSystem.Modules.Nbp.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.ActualRates;
using CreateInvoiceSystem.Modules.Nbp.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Nbp.Application.Handlers;
public class GetActualCurrencyRatesHandler(INbpApiRestService _nbpApiRestService) : IRequestHandler<GetActualCurrencyRatesRequest, GetActualCurrencyRatesResponse>
{
    public async Task<GetActualCurrencyRatesResponse> Handle(GetActualCurrencyRatesRequest request, CancellationToken cancellationToken)
    {
        var rates = await _nbpApiRestService.GetActualCurrencyRatesAsync(request.TableName, cancellationToken);
                
        return new GetActualCurrencyRatesResponse
        {
            Data = rates
        };
    }
}
