namespace CreateInvoiceSystem.Nbp.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Nbp.Application.Queries;
using CreateInvoiceSystem.Nbp.Application.RequestResponse.PreviousDatesRate;
using MediatR;

public class GetSeriesCurrencyRateFromToHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetSeriesCurrencyRateFromToRequest, GetSeriesCurrencyRateFromToResponse>
{
    public async Task<GetSeriesCurrencyRateFromToResponse> Handle(GetSeriesCurrencyRateFromToRequest request, CancellationToken cancellationToken)
    {
        GetSeriesCurrencyRateFromToQuery query = new(request.TableName, request.CurrencyCode, request.DateFrom, request.DateTo);

        var addresses = await queryExecutor.Execute(query);

        return new GetSeriesCurrencyRateFromToResponse
        {
            Data = addresses
        };
    }
}
