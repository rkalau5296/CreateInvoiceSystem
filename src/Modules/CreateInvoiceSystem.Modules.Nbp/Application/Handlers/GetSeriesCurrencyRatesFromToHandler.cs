namespace CreateInvoiceSystem.Modules.Nbp.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Nbp.Application.Queries;
using CreateInvoiceSystem.Modules.Nbp.Application.Options;
using MediatR;
using Microsoft.Extensions.Options;
using CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.PreviousDatesRates;

public class GetSeriesCurrencyRatesFromToHandler(IQueryExecutor queryExecutor, IOptions<NbpApiOptions> options) : IRequestHandler<GetSeriesCurrencyRatesFromToRequest, GetSeriesCurrencyRatesFromToResponse>
{
    public async Task<GetSeriesCurrencyRatesFromToResponse> Handle(GetSeriesCurrencyRatesFromToRequest request, CancellationToken cancellationToken)
    {
        GetSeriesCurrencyRatesFromToQuery query = new(request.TableName, request.DateFrom, request.DateTo, options.Value.BaseUrl);

        var addresses = await queryExecutor.Execute(query);

        return new GetSeriesCurrencyRatesFromToResponse
        {
            Data = addresses
        };
    }
}
