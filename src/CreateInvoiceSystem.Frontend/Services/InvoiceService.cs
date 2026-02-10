using CreateInvoiceSystem.Frontend.Models;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
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
            var url = $"api/Invoice?PageNumber={pageNumber}&PageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                url += $"&SearchTerm={Uri.EscapeDataString(searchTerm)}";
            }

            var response = await _http.GetFromJsonAsync<GetInvoicesResponse>(url);
            return response ?? new GetInvoicesResponse { Data = new List<InvoiceDto>(), TotalCount = 0 };
        }

        public async Task<InvoiceDto?> SaveInvoiceAsync(InvoiceDto invoice)
        {
            invoice.UserId = await GetUserIdFromToken();
            var response = await _http.PostAsJsonAsync("api/Invoice/create", invoice);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<InvoiceDto>();
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"API error: {error}");
        }

        public async Task UpdateInvoiceAsync(InvoiceDto invoice)
        {
            var response = await _http.PutAsJsonAsync($"api/Invoice/update/{invoice.InvoiceId}", invoice);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error: {error}");
            }
        }

        public async Task DeleteInvoiceAsync(int invoiceId)
        {
            var response = await _http.DeleteAsync($"api/Invoice/{invoiceId}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error: {error}");
            }
        }

        public async Task DownloadInvoicesCsvAsync()
        {
            var response = await _http.GetAsync("api/export/invoices");

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                await _js.InvokeVoidAsync("downloadFile", "faktury.csv", "text/csv", fileBytes);
            }
        }

        public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
        {
            var response = await _http.GetFromJsonAsync<GetInvoiceResponse>($"api/Invoice/{id}");
            return response?.Data;
        }

        public async Task<byte[]> DownloadInvoicePdfAsync(int id)
        {
            var response = await _http.GetAsync($"api/Invoice/{id}/pdf");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            return Array.Empty<byte>();
        }

        private async Task<int> GetUserIdFromToken()
        {
            var token = await _js.InvokeAsync<string>("sessionStorage.getItem", "authToken");
            if (string.IsNullOrEmpty(token)) return 0;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;

            return int.TryParse(claim, out var id) ? id : 0;
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