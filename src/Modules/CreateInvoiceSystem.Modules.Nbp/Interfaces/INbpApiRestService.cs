using CreateInvoiceSystem.Modules.Nbp.Application.DTO;

namespace CreateInvoiceSystem.Modules.Nbp.Interfaces;

public interface INbpApiRestService
{
    Task<CurrencyRatesTable> GetActualCurrencyRateAsync(string table, string currencyCode, CancellationToken cancellationToken);

    Task<List<CurrencyRatesTable>> GetActualCurrencyRatesAsync(string table, CancellationToken cancellationToken);

    Task<CurrencyRatesTable> GetSeriesCurrencyRateFromToAsync(string table, string currencyCode, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken);

    Task<List<CurrencyRatesTable>> GetSeriesCurrencyRatesFromToAsync(string table, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken);
}
