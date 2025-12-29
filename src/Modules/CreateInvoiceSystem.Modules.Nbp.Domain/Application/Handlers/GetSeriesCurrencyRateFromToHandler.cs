using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRate;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
public class GetSeriesCurrencyRateFromToHandler(IQueryExecutor queryExecutor, IOptions<NbpApiOptions> options, INbpApiRestService _nbpApiRestService) : IRequestHandler<GetSeriesCurrencyRateFromToRequest, GetSeriesCurrencyRateFromToResponse>
{
    public async Task<GetSeriesCurrencyRateFromToResponse> Handle(GetSeriesCurrencyRateFromToRequest request, CancellationToken cancellationToken)
    {
        GetSeriesCurrencyRateFromToQuery query = new(request.TableName, request.CurrencyCode, request.DateFrom, request.DateTo, options.Value.BaseUrl);

        var addresses = await queryExecutor.Execute(query, _nbpApiRestService, cancellationToken);

        return new GetSeriesCurrencyRateFromToResponse
        {
            Data = addresses
        };
    }
}
