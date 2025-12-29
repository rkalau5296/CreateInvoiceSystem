using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;
using CreateInvoiceSystem.Modules.Nbp.Domain.Inerfaces;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
public class GetSeriesCurrencyRatesFromToQuery(string table, DateTime dateFrom, DateTime dateTo, string baseUrl) : QueryBase<List<CurrencyRatesTable>, INbpApiRestService>
{
    public override async Task<List<CurrencyRatesTable>> Execute(INbpApiRestService _nbpApiRestService, CancellationToken cancellationToken)
    {
        return await _nbpApiRestService.GetSeriesCurrencyRatesFromToAsync(baseUrl, table, dateFrom, dateTo, cancellationToken);
    }
}
