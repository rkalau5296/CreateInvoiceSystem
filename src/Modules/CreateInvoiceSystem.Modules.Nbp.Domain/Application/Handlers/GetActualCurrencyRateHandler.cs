using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRate;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
public class GetActualCurrencyRateHandler(IQueryExecutor queryExecutor, IOptions<NbpApiOptions> options, INbpApiRestService _nbpApiRestService) : IRequestHandler<GetActualCurrencyRateRequest, GetActualCurrencyRateResponse>
{
    public async Task<GetActualCurrencyRateResponse> Handle(GetActualCurrencyRateRequest request, CancellationToken cancellationToken)
    {
        GetActualCurrencyRateQuery query = new(request.TableName, request.CurrencyCode, options.Value.BaseUrl);
        var address = await queryExecutor.Execute(query, _nbpApiRestService, cancellationToken);
        return new GetActualCurrencyRateResponse
        {
            Data = address
        };
    }
}
