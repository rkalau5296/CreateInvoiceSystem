using CreateInvoiceSystem.Frontend.Models;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Frontend.Services
{
    public class ClientService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public ClientService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        public async Task<GetClientsResponse> GetClientsAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            await SetAuthHeader();

            var url = $"api/Client?PageNumber={pageNumber}&PageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                url += $"&SearchTerm={Uri.EscapeDataString(searchTerm)}";
            }

            var response = await _http.GetFromJsonAsync<GetClientsResponse>(url);

            return response ?? new GetClientsResponse { Data = new List<ClientDto>(), TotalCount = 0 };
        }

        public class GetClientsResponse
        {
            [JsonPropertyName("data")]
            public List<ClientDto> Data { get; set; } = new();
            public int TotalCount { get; set; }
            public bool Success { get; set; }
        }

        public async Task SaveClientAsync(ClientDto client)
        {
            await SetAuthHeader();

            client.UserId = await GetUserIdFromToken();

            var response = await _http.PostAsJsonAsync("api/Client/create", client);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateClientAsync(ClientDto client)
        {
            await SetAuthHeader();
            
            var updateDto = new
            {
                client.ClientId,
                client.Name,
                client.Nip,
                Address = new
                {
                    client.Address.Street,
                    client.Address.Number,
                    client.Address.City,
                    client.Address.PostalCode,
                    client.Address.Country
                },
                client.UserId,
                client.IsDeleted
            };

            var response = await _http.PutAsJsonAsync($"api/Client/update/{client.ClientId}", updateDto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API zwróciło błąd: {error}");
            }
        }

        public async Task DeleteClientAsync(int clientId)
        {
            await SetAuthHeader();
            var response = await _http.DeleteAsync($"api/Client/{clientId}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Nie udało się usunąć klienta: {error}");
            }
        }

        private async Task SetAuthHeader()
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<int> GetUserIdFromToken()
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (string.IsNullOrEmpty(token)) return 0;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;

            return int.TryParse(claim, out var id) ? id : 0;
        }
        
        public async Task DownloadClientsCsvAsync()
        {
            await SetAuthHeader();
            var response = await _http.GetAsync("api/export/clients");

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                await _js.InvokeVoidAsync("downloadFile", "klienci.csv", "text/csv", fileBytes);
            }
        }
    }
}