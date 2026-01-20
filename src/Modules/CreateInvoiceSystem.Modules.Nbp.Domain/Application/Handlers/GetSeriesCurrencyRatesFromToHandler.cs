using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRates;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
public class GetSeriesCurrencyRatesFromToHandler(IQueryExecutor queryExecutor, IOptions<NbpApiOptions> options, INbpApiRestService _nbpApiRestService) : IRequestHandler<GetSeriesCurrencyRatesFromToRequest, GetSeriesCurrencyRatesFromToResponse>
{
    public async Task<GetSeriesCurrencyRatesFromToResponse> Handle(GetSeriesCurrencyRatesFromToRequest request, CancellationToken cancellationToken)
    {
        GetSeriesCurrencyRatesFromToQuery query = new(request.TableName, request.DateFrom, request.DateTo, options.Value.BaseUrl);

        var addresses = await queryExecutor.Execute(query, _nbpApiRestService, cancellationToken);

        return new GetSeriesCurrencyRatesFromToResponse
        {
            Data = addresses
        };
    }
}
