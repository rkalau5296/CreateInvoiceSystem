using CreateInvoiceSystem.Frontend.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Frontend.Services
{
    public class InvoiceService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public InvoiceService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        public async Task<GetInvoicesResponse> GetInvoicesAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = new Dictionary<string, string?>
            {
                ["pageNumber"] = pageNumber.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["searchTerm"] = searchTerm
            };

            var url = QueryHelpers.AddQueryString("api/Invoice", query);
            var response = await _http.GetFromJsonAsync<GetInvoicesResponse>(url);

            return response ?? new GetInvoicesResponse();
        }

        public async Task<InvoiceDto?> SaveInvoiceAsync(InvoiceDto invoice)
        {
            var response = await _http.PostAsJsonAsync("api/Invoice/create", invoice);
            await response.EnsureSuccessOrThrowApiExceptionAsync();

            var result = await response.Content.ReadFromJsonAsync<GetInvoiceResponse>();
            return result?.Data;
        }

        public async Task UpdateInvoiceAsync(InvoiceDto invoice)
        {
            var response = await _http.PutAsJsonAsync($"api/Invoice/update/{invoice.InvoiceId}", invoice);
            await response.EnsureSuccessOrThrowApiExceptionAsync();
        }

        public async Task DeleteInvoiceAsync(int invoiceId)
        {
            var response = await _http.DeleteAsync($"api/Invoice/{invoiceId}");
            await response.EnsureSuccessOrThrowApiExceptionAsync();
        }

        public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
        {
            var response = await _http.GetAsync($"api/Invoice/{id}");
            await response.EnsureSuccessOrThrowApiExceptionAsync();

            var result = await response.Content.ReadFromJsonAsync<GetInvoiceResponse>();
            return result?.Data;
        }

        public async Task<byte[]> DownloadInvoicePdfAsync(int id)
        {
            var response = await _http.GetAsync($"api/Invoice/{id}/pdf");
            await response.EnsureSuccessOrThrowApiExceptionAsync();

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task DownloadInvoicesCsvAsync()
        {
            var response = await _http.GetAsync("api/export/invoices");
            await response.EnsureSuccessOrThrowApiExceptionAsync();

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            await _js.InvokeVoidAsync("downloadFile", "faktury.csv", "text/csv", fileBytes);
        }

        public class GetInvoicesResponse
        {
            [JsonPropertyName("data")]
            public List<InvoiceDto> Data { get; set; } = new();

            [JsonPropertyName("totalCount")]
            public int TotalCount { get; set; }

            public bool Success { get; set; }
        }

        public class GetInvoiceResponse : ResponseBase<InvoiceDto> { }
    }
}