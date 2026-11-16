using CreateInvoiceSystem.Frontend.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Json;

namespace CreateInvoiceSystem.Frontend.Services;

public sealed class KsefService(HttpClient http, IJSRuntime js)
{
    public async Task<bool> IsConfiguredAsync()
    {
        using var response = await http.GetAsync("api/Ksef/settings");
        await response.EnsureSuccessOrThrowApiExceptionAsync();
        var result = await response.Content.ReadFromJsonAsync<KsefSettingsResponse>()
            ?? throw new InvalidOperationException("API zwróciło pustą odpowiedź ustawień KSeF.");
        return result.IsConfigured;
    }

    public async Task SaveTokenAsync(string token)
    {
        using var response = await http.PutAsJsonAsync("api/Ksef/settings", new { Token = token });
        await response.EnsureSuccessOrThrowApiExceptionAsync();
    }

    public async Task DeleteTokenAsync()
    {
        using var response = await http.DeleteAsync("api/Ksef/settings");
        await response.EnsureSuccessOrThrowApiExceptionAsync();
    }

    public async Task<KsefInvoicesResponse> GetIncomingInvoicesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int pageNumber,
        int pageSize)
    {
        var query = new Dictionary<string, string?>
        {
            ["fromDate"] = fromDate.ToString("yyyy-MM-dd"),
            ["toDate"] = toDate.ToString("yyyy-MM-dd"),
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString()
        };
        using var response = await http.GetAsync(QueryHelpers.AddQueryString("api/Ksef/invoices", query));
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        return await response.Content.ReadFromJsonAsync<KsefInvoicesResponse>()
            ?? throw new InvalidOperationException("API zwróciło pustą odpowiedź z listą faktur KSeF.");
    }

    public async Task<KsefInvoiceDetailsDto> GetInvoiceDetailsAsync(string ksefNumber)
    {
        using var response = await http.GetAsync($"api/Ksef/invoices/{Uri.EscapeDataString(ksefNumber)}");
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        return await response.Content.ReadFromJsonAsync<KsefInvoiceDetailsDto>()
            ?? throw new InvalidOperationException("API zwróciło pustą odpowiedź ze szczegółami faktury KSeF.");
    }

    public async Task<KsefStatusDto?> GetInvoiceStatusAsync(int invoiceId)
    {
        using var response = await http.GetAsync($"api/Ksef/{invoiceId}/status");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        return await response.Content.ReadFromJsonAsync<KsefStatusDto>();
    }

    public async Task SendInvoiceAsync(int invoiceId)
    {
        using var response = await http.PostAsync($"api/Ksef/{invoiceId}/send", null);
        await response.EnsureSuccessOrThrowApiExceptionAsync();
    }

    public async Task DownloadInvoiceXmlAsync(string ksefNumber)
    {
        using var response = await http.GetAsync($"api/Ksef/invoices/{Uri.EscapeDataString(ksefNumber)}/xml");
        await response.EnsureSuccessOrThrowApiExceptionAsync();

        var xmlBytes = await response.Content.ReadAsByteArrayAsync();
        await js.InvokeVoidAsync("downloadFile", $"KSeF_{ksefNumber}.xml", "application/xml", xmlBytes);
    }

    private sealed class KsefSettingsResponse
    {
        public bool IsConfigured { get; set; }
    }
}
