using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Nbp.Application.DTO;
using CreateInvoiceSystem.Nbp.Application.Queries;
using CreateInvoiceSystem.Nbp.Application.RequestResponse;
using MediatR;

namespace CreateInvoiceSystem.Nbp.Application.Handlers;

public class GetActualCurrencyRatesHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetActualCurrencyRatesRequest, GetActualCurrencyRatesResponse>
{
    public async Task<GetActualCurrencyRatesResponse> Handle(GetActualCurrencyRatesRequest request, CancellationToken cancellationToken)
    {
        GetActualCurrencyRatesQuery query = new(request.TableName);

        var addresses = await queryExecutor.Execute(query);

        return new GetActualCurrencyRatesResponse
        {
            Data = addresses
        };
    }
}
