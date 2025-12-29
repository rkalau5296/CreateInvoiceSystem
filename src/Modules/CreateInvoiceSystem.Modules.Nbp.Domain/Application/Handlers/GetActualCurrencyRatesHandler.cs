using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRates;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
public class GetActualCurrencyRatesHandler(IQueryExecutor queryExecutor, IOptions<NbpApiOptions> options, INbpApiRestService _nbpApiRestService) : IRequestHandler<GetActualCurrencyRatesRequest, GetActualCurrencyRatesResponse>
{
    public async Task<GetActualCurrencyRatesResponse> Handle(GetActualCurrencyRatesRequest request, CancellationToken cancellationToken)
    {
        GetActualCurrencyRatesQuery query = new(request.TableName, options.Value.BaseUrl);

        var addresses = await queryExecutor.Execute(query, _nbpApiRestService, cancellationToken);

        return new GetActualCurrencyRatesResponse
        {
            Data = addresses
        };
    }
}
