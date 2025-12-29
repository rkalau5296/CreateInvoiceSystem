using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
public class GetActualCurrencyRatesQuery(string table, string baseUrl) : QueryBase<List<CurrencyRatesTable>, INbpApiRestService>
{
    public override async Task<List<CurrencyRatesTable>> Execute(INbpApiRestService _nbpApiRestService, CancellationToken cancellationToken)
    {
        return await _nbpApiRestService.GetActualCurrencyRatesAsync(baseUrl, table, cancellationToken);
    }
}
