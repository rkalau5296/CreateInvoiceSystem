namespace CreateInvoiceSystem.Nbp.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Nbp.Application.Queries;
using CreateInvoiceSystem.Nbp.Application.RequestResponse.PreviousDatesRates;
using MediatR;

public class GetSeriesCurrencyRatesFromToHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetSeriesCurrencyRatesFromToRequest, GetSeriesCurrencyRatesFromToResponse>
{
    public async Task<GetSeriesCurrencyRatesFromToResponse> Handle(GetSeriesCurrencyRatesFromToRequest request, CancellationToken cancellationToken)
    {
        GetSeriesCurrencyRatesFromToQuery query = new(request.TableName, request.DateFrom, request.DateTo);

        var addresses = await queryExecutor.Execute(query);

        return new GetSeriesCurrencyRatesFromToResponse
        {
            Data = addresses
        };
    }
}
