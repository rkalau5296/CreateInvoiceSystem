namespace CreateInvoiceSystem.Nbp.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Nbp.Application.Options;
using CreateInvoiceSystem.Nbp.Application.Queries;
using CreateInvoiceSystem.Nbp.Application.RequestResponse.PreviousDatesRates;
using MediatR;
using Microsoft.Extensions.Options;

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
