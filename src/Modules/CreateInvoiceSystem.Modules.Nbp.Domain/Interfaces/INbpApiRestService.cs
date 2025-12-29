using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;

public interface INbpApiRestService
{
    Task<CurrencyRatesTable> GetActualCurrencyRateAsync(string baseUrl, string table, string currencyCode, CancellationToken cancellationToken);

    Task<List<CurrencyRatesTable>> GetActualCurrencyRatesAsync(string baseUrl, string table, CancellationToken cancellationToken);

    Task<CurrencyRatesTable> GetSeriesCurrencyRateFromToAsync(string baseUrl, string table, string currencyCode, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken);

    Task<List<CurrencyRatesTable>> GetSeriesCurrencyRatesFromToAsync(string baseUrl, string table, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken);
}
