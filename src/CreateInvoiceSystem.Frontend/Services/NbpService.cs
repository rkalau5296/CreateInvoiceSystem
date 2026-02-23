using CreateInvoiceSystem.Frontend.Models;
using System.Net.Http.Json;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CreateInvoiceSystem.Frontend.Services;

public class NbpService
{
    private readonly HttpClient _httpClient;

    public NbpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CurrencyRatesTable?> GetCurrentTableAsync(string tableName)
    {
        var response = await _httpClient.GetAsync($"CurrencyRates/{tableName}");
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        var dto = await response.Content.ReadFromJsonAsync<GetActualCurrencyRatesResponse>();
        return dto?.Data?.FirstOrDefault();
    }

    public async Task<List<CurrencyRatesTable>> GetTableHistoryAsync(string tableName, DateTime from, DateTime to)
    {
        var dateFrom = from.ToString("yyyy-MM-dd");
        var dateTo = to.ToString("yyyy-MM-dd");

        var response = await _httpClient.GetAsync($"CurrencyRates/{tableName}/{dateFrom}/{dateTo}");
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        var dto = await response.Content.ReadFromJsonAsync<GetActualCurrencyRatesResponse>();
        return dto?.Data ?? new List<CurrencyRatesTable>();
    }

    public async Task<CurrencyRatesTable?> GetSpecificCurrencyRateAsync(string tableName, string code)
    {
        var response = await _httpClient.GetAsync($"CurrencyRates/{tableName}/{code}");
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        var dto = await response.Content.ReadFromJsonAsync<GetSingleCurrencyRateResponse>();
        return dto?.Data;
    }

    public async Task<CurrencyRatesTable?> GetSpecificCurrencyHistoryAsync(string tableName, string code, DateTime from, DateTime to)
    {
        var dFrom = from.ToString("yyyy-MM-dd");
        var dTo = to.ToString("yyyy-MM-dd");

        var response = await _httpClient.GetAsync($"CurrencyRates/{tableName}/{code}/{dFrom}/{dTo}");
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        var dto = await response.Content.ReadFromJsonAsync<GetSingleCurrencyRateResponse>();
        return dto?.Data;
    }
}