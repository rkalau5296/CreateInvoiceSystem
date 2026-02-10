using CreateInvoiceSystem.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Frontend.Services
{
    public class ProductService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private readonly NavigationManager _navigationManager;

        public ProductService(HttpClient http, IJSRuntime js, NavigationManager navigationManager)
        {
            _http = http;
            _js = js;
            _navigationManager = navigationManager;
        }

        public async Task<GetProductsResponse> GetProductsAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var url = $"api/Product?PageNumber={pageNumber}&PageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                url += $"&SearchTerm={Uri.EscapeDataString(searchTerm)}";
            }

            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GetProductsResponse>() ?? new GetProductsResponse();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _navigationManager.NavigateTo("/login");
            }

            return new GetProductsResponse();
        }

        public async Task SaveProductAsync(ProductDto product)
        {
            product.UserId = await GetUserIdFromToken();
            await _http.PostAsJsonAsync("api/Product/create", product);
        }

        public async Task UpdateProductAsync(ProductDto product)
        {
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
                throw new Exception($"API error: {error}");
            }
        }

        public async Task DeleteProductAsync(int productId)
        {
            var response = await _http.DeleteAsync($"api/Product/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error: {error}");
            }
        }

        public async Task DownloadProductsCsvAsync()
        {
            var response = await _http.GetAsync("api/export/products");

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                await _js.InvokeVoidAsync("downloadFile", "produkty.csv", "text/csv", fileBytes);
            }
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

        public class GetProductsResponse
        {
            [JsonPropertyName("data")]
            public List<ProductDto> Data { get; set; } = new();
            public int TotalCount { get; set; }
            public bool Success { get; set; }
        }
    }
}