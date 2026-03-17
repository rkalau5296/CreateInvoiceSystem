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

    private static readonly HashSet<string> _tableA = new(StringComparer.OrdinalIgnoreCase)
    {
        "THB","USD","AUD","HKD","CAD","NZD","SGD","EUR","HUF","CHF","GBP","UAH","JPY",
        "CZK","DKK","ISK","NOK","SEK","RON","TRY","ILS","CLP","PHP","MXN","ZAR","BRL",
        "MYR","IDR","INR","KRW","CNY","XDR"
    };

    private static readonly HashSet<string> _tableB = new(StringComparer.OrdinalIgnoreCase)
    {
        "AFN","MGA","PAB","ETB","VES","BOB","CRC","SVC","NIO","GMD",
        "MKD","DZD","BHD","IQD","JOD","KWD","LYD","RSD","TND","MAD",
        "AED","STN","BSD","BBD","BZD","BND","FJD","GYD","JMD","LRD",
        "NAD","SRD","TTD","XCD","SBD","VND","AMD","CVE","AWG","BIF",
        "XOF","XAF","XPF","DJF","GNF","KMF","CDF","RWF","EGP","GIP",
        "LBP","SSP","SDG","SYP","GHS","HTG","PYG","XCG","PGK","LAK",
        "MWK","ZMW","AOA","MMK","GEL","MDL","ALL","HNL","SLE","SZL",
        "LSL","AZN","MZN","NGN","ERN","TWD","TMT","MRU","TOP","MOP",
        "ARS","DOP","COP","CUP","UYU","BWP","GTQ","IRR","YER","QAR",
        "OMR","SAR","KHR","BYN","RUB","LKR","MVR","MUR","NPR","PKR",
        "SCR","PEN","KGS","TJS","UZS","KES","SOS","TZS","UGX","BDT",
        "WST","KZT","MNT","VUV","BAM","ZWG"
    };

    private static readonly HashSet<string> _tableC = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD","AUD","CAD","EUR","HUF","CHF","GBP","JPY",
        "CZK","DKK","NOK","SEK", "XDR"
    };

    private static readonly Dictionary<string, HashSet<string>> _tableMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["A"] = _tableA,
        ["B"] = _tableB,
        ["C"] = _tableC
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
            throw new ArgumentException("Data 'from' nie może być późniejsza niż data 'to'.");

        var dateFrom = from.ToString("yyyy-MM-dd");
        var dateTo = to.ToString("yyyy-MM-dd");

        var response = await _httpClient.GetAsync($"CurrencyRates/{tableName}/{dateFrom}/{dateTo}");
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        var dto = await response.Content.ReadFromJsonAsync<GetActualCurrencyRatesResponse>();
        return dto?.Data ?? new List<CurrencyRatesTable>();
    }

    public async Task<CurrencyRatesTable?> GetSpecificCurrencyRateAsync(string tableName, string code)
    {
        ValidateCurrencyCode(tableName, code);

        var normalizedCode = code.Trim().ToUpperInvariant();
        var response = await _httpClient.GetAsync($"CurrencyRates/{tableName}/{normalizedCode}");
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        var dto = await response.Content.ReadFromJsonAsync<GetSingleCurrencyRateResponse>();
        return dto?.Data;
    }

    public async Task<CurrencyRatesTable?> GetSpecificCurrencyHistoryAsync(string tableName, string code, DateTime from, DateTime to)
    {
        if (from > to)
            throw new ArgumentException("Data 'from' nie może być późniejsza niż data 'to'.");

        ValidateCurrencyCode(tableName, code);

        var dFrom = from.ToString("yyyy-MM-dd");
        var dTo = to.ToString("yyyy-MM-dd");
        var normalizedCode = code.Trim().ToUpperInvariant();

        var response = await _httpClient.GetAsync($"CurrencyRates/{tableName}/{normalizedCode}/{dFrom}/{dTo}");
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        var dto = await response.Content.ReadFromJsonAsync<GetSingleCurrencyRateResponse>();
        return dto?.Data;
    }

    private static void ValidateCurrencyCode(string? tableName, string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Kod waluty nie może być pusty.");

        var trimmed = code.Trim().ToUpperInvariant();

        if (!_validCurrencyCodes.Contains(trimmed))
            throw new ArgumentException("Podany kod waluty jest błędny lub nie występuje w NBP.");

        if (string.IsNullOrWhiteSpace(tableName))
            return;
        
        var key = tableName.Trim().ToUpperInvariant();
        if (key.Length != 1 || (key != "A" && key != "B" && key != "C"))
            throw new ArgumentException("Nieznana tabela.");

        if (!_tableMap.TryGetValue(key, out var set))
            throw new ArgumentException("Nieznana tabela.");

        if (!set.Contains(trimmed))
            throw new ArgumentException("Kod waluty z poza tabeli");
    }
}