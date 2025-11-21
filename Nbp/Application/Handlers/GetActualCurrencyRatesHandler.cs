namespace CreateInvoiceSystem.Nbp.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Nbp.Application.Options;
using CreateInvoiceSystem.Nbp.Application.Queries;
using CreateInvoiceSystem.Nbp.Application.RequestResponse.ActualRates;
using MediatR;
using Microsoft.Extensions.Options;

public class GetActualCurrencyRatesHandler(IQueryExecutor queryExecutor, IOptions<NbpApiOptions> options) : IRequestHandler<GetActualCurrencyRatesRequest, GetActualCurrencyRatesResponse>
{
    public async Task<GetActualCurrencyRatesResponse> Handle(GetActualCurrencyRatesRequest request, CancellationToken cancellationToken)
    {
        GetActualCurrencyRatesQuery query = new(request.TableName, options.Value.BaseUrl);

        var addresses = await queryExecutor.Execute(query);

        return new GetActualCurrencyRatesResponse
        {
            Data = addresses
        };
    }
}
