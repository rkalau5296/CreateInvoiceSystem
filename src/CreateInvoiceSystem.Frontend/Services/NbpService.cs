using CreateInvoiceSystem.Frontend.Models;
using System;
using System.Net.Http.Json;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CreateInvoiceSystem.Frontend.Services;

public class NbpService
{
    private readonly HttpClient _httpClient;

    private static readonly HashSet<string> _validCurrencyCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD", "AUD", "CAD", "EUR", "HUF", "CHF", "GBP", "JPY", "CZK", "DKK",
        "NOK", "SEK", "XDR", "UAH", "RON", "TRY", "ILS", "CLP", "PHP", "MXN",
        "ZAR", "BRL", "MYR", "IDR", "INR", "KRW", "CNY", "THB", "HKD", "NZD",
        "SGD", "AED", "AFN", "ALL", "AMD", "ANG", "AOA", "ARS", "BGN", "BHD",
        "BND", "BOB", "BWP", "BZD", "CLF", "COP", "CRC", "CUP", "CVE", "DJF",
        "DOP", "DZD", "EGP", "ETB", "FJD", "GEL", "GHS", "GMD", "GNF", "GTQ",
        "GYD", "HNL", "HRK", "HTG", "IQD", "IRR", "ISK", "JMD", "JOD", "KES",
        "KGS", "KHR", "KMF", "KPW", "KWD", "KYD", "KZT", "LAK", "LBP", "LKR",
        "LRD", "LSL", "LYD", "MAD", "MDL", "MGA", "MKD", "MMK", "MNT", "MOP",
        "MRO", "MUR", "MVR", "MWK", "NAD", "NGN", "NIO", "NPR", "OMR", "PAB",
        "PEN", "PGK", "PKR", "PYG", "QAR", "RSD", "RUB", "RWF", "SAR", "SBD",
        "SCR", "SDG", "SHP", "SLL", "SOS", "SRD", "STD", "SYP", "SZL", "TJS",
        "TMT", "TND", "TOP", "TTD", "TWD", "TZS", "UGX", "UYU", "UZS", "VEF",
        "VND", "VUV", "WST", "XAF", "XCD", "XOF", "XPF", "YER", "ZMW"
    };

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
        if (from > to)
            throw new ArgumentException("Data 'from' nie może być późniejsza niż data 'to'.", nameof(from));

        var dateFrom = from.ToString("yyyy-MM-dd");
        var dateTo = to.ToString("yyyy-MM-dd");

        var response = await _httpClient.GetAsync($"CurrencyRates/{tableName}/{dateFrom}/{dateTo}");
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        var dto = await response.Content.ReadFromJsonAsync<GetActualCurrencyRatesResponse>();
        return dto?.Data ?? new List<CurrencyRatesTable>();
    }

    public async Task<CurrencyRatesTable?> GetSpecificCurrencyRateAsync(string tableName, string code)
    {
        ValidateCurrencyCode(code);

        var normalizedCode = code.Trim().ToUpperInvariant();
        var response = await _httpClient.GetAsync($"CurrencyRates/{tableName}/{normalizedCode}");
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        var dto = await response.Content.ReadFromJsonAsync<GetSingleCurrencyRateResponse>();
        return dto?.Data;
    }

    public async Task<CurrencyRatesTable?> GetSpecificCurrencyHistoryAsync(string tableName, string code, DateTime from, DateTime to)
    {
        if (from > to)
            throw new ArgumentException("Data 'from' nie może być późniejsza niż data 'to'.", nameof(from));

        ValidateCurrencyCode(code);

        var dFrom = from.ToString("yyyy-MM-dd");
        var dTo = to.ToString("yyyy-MM-dd");
        var normalizedCode = code.Trim().ToUpperInvariant();

        var response = await _httpClient.GetAsync($"CurrencyRates/{tableName}/{normalizedCode}/{dFrom}/{dTo}");
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        var dto = await response.Content.ReadFromJsonAsync<GetSingleCurrencyRateResponse>();
        return dto?.Data;
    }

    private static void ValidateCurrencyCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Kod waluty nie może być pusty.");

        var trimmed = code.Trim();
        if (!_validCurrencyCodes.Contains(trimmed))
            throw new ArgumentException("Podany kod waluty jest błędny lub nie występuje w NBP.");
    }
}