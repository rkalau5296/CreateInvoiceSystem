using CreateInvoiceSystem.Modules.Nbp.Application.DTO;
using CreateInvoiceSystem.Modules.Nbp.Interfaces;
using System.Net;

namespace CreateInvoiceSystem.API.RestServices;

public sealed class NbpApiRestService(HttpClient httpClient)
    : INbpApiRestService
{
    public async Task<CurrencyRatesTable> GetActualCurrencyRateAsync(
    string table,
    string currencyCode,
    CancellationToken cancellationToken)
    {
        var uri = $"rates/{table}/{currencyCode}?format=json";

        return await GetRequiredAsync<CurrencyRatesTable>(
            uri,
            cancellationToken);
    }

    public async Task<List<CurrencyRatesTable>> GetActualCurrencyRatesAsync(
        string table,
        CancellationToken cancellationToken)
    {
        var uri = $"tables/{table}?format=json";

        return await GetRequiredAsync<List<CurrencyRatesTable>>(
            uri,
            cancellationToken);
    }

    public async Task<CurrencyRatesTable> GetSeriesCurrencyRateFromToAsync(
        string table,
        string currencyCode,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken)
    {
        var uri =
            $"rates/{table}/{currencyCode}/" +
            $"{dateFrom:yyyy-MM-dd}/{dateTo:yyyy-MM-dd}?format=json";

        var result = await GetRequiredAsync<List<CurrencyRatesTable>>(
            uri,
            cancellationToken);

        return result.Single();
    }

    public async Task<List<CurrencyRatesTable>> GetSeriesCurrencyRatesFromToAsync(
        string table,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken)
    {
        var uri =
            $"tables/{table}/" +
            $"{dateFrom:yyyy-MM-dd}/{dateTo:yyyy-MM-dd}?format=json";

        return await GetRequiredAsync<List<CurrencyRatesTable>>(
            uri,
            cancellationToken);
    }

    private async Task<T> GetRequiredAsync<T>(
        string uri,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            uri,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException(
                $"NBP API error: {(int)response.StatusCode} " +
                $"{response.ReasonPhrase}");
        }

        var result = await response.Content.ReadFromJsonAsync<T>(
            cancellationToken);

        return result
            ?? throw new InvalidOperationException(
                "NBP API returned an empty response.");
    }
}