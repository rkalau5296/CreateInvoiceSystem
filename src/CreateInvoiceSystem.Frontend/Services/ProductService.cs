using CreateInvoiceSystem.Frontend.Models;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Frontend.Services
{
    public class ProductService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public ProductService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        public async Task<GetProductsResponse> GetProductsAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            await SetAuthHeader();
            
            var url = $"api/Product?PageNumber={pageNumber}&PageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                url += $"&SearchTerm={Uri.EscapeDataString(searchTerm)}";
            }

            var response = await _http.GetFromJsonAsync<GetProductsResponse>(url);

            return response ?? new GetProductsResponse { Data = new List<ProductDto>(), TotalCount = 0 };
        }

        public class GetProductsResponse
        {
            [JsonPropertyName("data")]
            public List<ProductDto> Data { get; set; } = new();

            public int TotalCount { get; set; } // Dodaj to pole
            public bool Success { get; set; }
        }

        public async Task SaveProductAsync(ProductDto product)
        {
            await SetAuthHeader();
            
            product.UserId = await GetUserIdFromToken();

            var response = await _http.PostAsJsonAsync("api/Product/create", product);
            response.EnsureSuccessStatusCode();
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

        public async Task DeleteProductAsync(int productId)
        {
            await SetAuthHeader(); 
            var response = await _http.DeleteAsync($"api/Product/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Nie udało się usunąć produktu: {error}");
            }
        }

        public async Task UpdateProductAsync(ProductDto product)
        {
            await SetAuthHeader();
            
            var updateDto = new
            {
                product.ProductId,
                product.Name,
                product.Description,
                product.Value,
                product.UserId,
                product.IsDeleted
            };
            
            var response = await _http.PutAsJsonAsync($"api/Product/update/{product.ProductId}", updateDto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API zwróciło błąd: {error}");
            }
        }
        public async Task DownloadProductsCsvAsync()
        {
            await SetAuthHeader();            
            var response = await _http.GetAsync("api/export/products");

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                
                await _js.InvokeVoidAsync("downloadFile", "produkty.csv", "text/csv", fileBytes);
            }
        }
    }
}
