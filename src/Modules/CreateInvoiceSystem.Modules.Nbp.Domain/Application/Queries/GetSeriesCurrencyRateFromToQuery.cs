using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
public class GetSeriesCurrencyRateFromToQuery(string table, string currencyCode, DateTime dateFrom, DateTime dateTo, string baseUrl) : QueryBase<CurrencyRatesTable, INbpApiRestService>
{
    public override async Task<CurrencyRatesTable> Execute(INbpApiRestService _nbpApiRestService, CancellationToken cancellationToken)
    {
        return await _nbpApiRestService.GetSeriesCurrencyRateFromToAsync(baseUrl, table, currencyCode, dateFrom, dateTo, cancellationToken);
    }
}
