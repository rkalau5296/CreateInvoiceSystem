using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRates;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
public class GetActualCurrencyRatesHandler(IOptions<NbpApiOptions> options, INbpApiRestService _nbpApiRestService) : IRequestHandler<GetActualCurrencyRatesRequest, GetActualCurrencyRatesResponse>
{
    public async Task<GetActualCurrencyRatesResponse> Handle(GetActualCurrencyRatesRequest request, CancellationToken cancellationToken)
    {
        var rates = await _nbpApiRestService.GetActualCurrencyRatesAsync(options.Value.BaseUrl, request.TableName, cancellationToken);
                
        return new GetActualCurrencyRatesResponse
        {
            Data = rates
        };
    }
}
