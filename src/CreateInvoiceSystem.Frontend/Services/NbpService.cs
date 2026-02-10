using CreateInvoiceSystem.Frontend.Models;
using System.Net.Http.Json;

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
        var response = await _httpClient.GetFromJsonAsync<GetActualCurrencyRatesResponse>($"CurrencyRates/{tableName}");
        return response?.Data?.FirstOrDefault();
    }

    public async Task<List<CurrencyRatesTable>> GetTableHistoryAsync(string tableName, DateTime from, DateTime to)
    {
        var dateFrom = from.ToString("yyyy-MM-dd");
        var dateTo = to.ToString("yyyy-MM-dd");

        var response = await _httpClient.GetFromJsonAsync<GetActualCurrencyRatesResponse>(
            $"CurrencyRates/{tableName}/{dateFrom}/{dateTo}");

        return response?.Data ?? new List<CurrencyRatesTable>();
    }

    public async Task<CurrencyRatesTable?> GetSpecificCurrencyRateAsync(string tableName, string code)
    {
        var response = await _httpClient.GetFromJsonAsync<GetSingleCurrencyRateResponse>($"CurrencyRates/{tableName}/{code}");
        return response?.Data;
    }

    public async Task<CurrencyRatesTable?> GetSpecificCurrencyHistoryAsync(string tableName, string code, DateTime from, DateTime to)
    {
        var dFrom = from.ToString("yyyy-MM-dd");
        var dTo = to.ToString("yyyy-MM-dd");
        var response = await _httpClient.GetFromJsonAsync<GetSingleCurrencyRateResponse>($"CurrencyRates/{tableName}/{code}/{dFrom}/{dTo}");
        return response?.Data;
    }
}