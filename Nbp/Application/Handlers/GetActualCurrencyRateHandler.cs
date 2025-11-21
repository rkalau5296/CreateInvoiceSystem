namespace CreateInvoiceSystem.Nbp.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Nbp.Application.Queries;
using CreateInvoiceSystem.Nbp.Application.RequestResponse.ActualRate;
using MediatR;

public class GetActualCurrencyRateHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetActualCurrencyRateRequest, GetActualCurrencyRateResponse>
{
    public async Task<GetActualCurrencyRateResponse> Handle(GetActualCurrencyRateRequest request, CancellationToken cancellationToken)
    {
        GetActualCurrencyRateQyuery query = new(request.TableName, request.CurrencyCode);
        var address = await queryExecutor.Execute(query);
        return new GetActualCurrencyRateResponse
        {
            Data = address
        };
    }
}
