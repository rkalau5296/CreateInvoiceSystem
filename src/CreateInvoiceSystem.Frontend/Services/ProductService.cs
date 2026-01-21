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

        public async Task<List<ProductDto>> GetProductsAsync()
        {
            await SetAuthHeader();
            
            await _http.GetStringAsync("Products");            
            
            var response = await _http.GetFromJsonAsync<GetProductsResponse>("Products");

            return response?.Value ?? new List<ProductDto>();
        }

        public class GetProductsResponse
        {
            [JsonPropertyName("data")] 
            public List<ProductDto> Value { get; set; } = new();

            public bool Success { get; set; }
        }

        public async Task SaveProductAsync(ProductDto product)
        {
            await SetAuthHeader();
            
            product.UserId = await GetUserIdFromToken();

            var response = await _http.PostAsJsonAsync("Product/create", product);
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
            var response = await _http.DeleteAsync($"Product/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Nie udało się usunąć produktu: {error}");
            }
        }
    }
}
