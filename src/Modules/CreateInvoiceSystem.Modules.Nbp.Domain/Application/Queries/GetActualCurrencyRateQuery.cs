using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;
using CreateInvoiceSystem.Modules.Nbp.Domain.Inerfaces;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
public class GetActualCurrencyRateQuery(string table, string currencyCode, string baseUrl) : QueryBase<CurrencyRatesTable, INbpApiRestService>
{
    public override async Task<CurrencyRatesTable> Execute(INbpApiRestService _nbpApiRestService, CancellationToken cancellationToken)
    {
        return await _nbpApiRestService.GetActualCurrencyRateAsync(baseUrl, table, currencyCode, cancellationToken); ;
    }
}