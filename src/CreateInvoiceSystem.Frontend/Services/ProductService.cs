using CreateInvoiceSystem.Frontend.Models;
using Microsoft.AspNetCore.Components;
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
        private readonly NavigationManager _navigationManager;
        private readonly AuthService _authService;

        public ProductService(HttpClient http, IJSRuntime js, NavigationManager navigationManager, AuthService authService)
        {
            _http = http;
            _js = js;
            _navigationManager = navigationManager;
            _authService = authService;
        }

        public async Task<GetProductsResponse> GetProductsAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            await SetAuthHeader();

            var url = $"api/Product?PageNumber={pageNumber}&PageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                url += $"&SearchTerm={Uri.EscapeDataString(searchTerm)}";
            }

            var response = await _http.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var isRefreshed = await _authService.RefreshTokenAsync();

                if (isRefreshed)
                {
                    await SetAuthHeader();
                    response = await _http.GetAsync(url);
                }
                else
                {
                    _navigationManager.NavigateTo("/login");
                    return new GetProductsResponse { Data = new List<ProductDto>(), TotalCount = 0 };
                }
            }

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GetProductsResponse>();
                return result ?? new GetProductsResponse { Data = new List<ProductDto>(), TotalCount = 0 };
            }

            return new GetProductsResponse { Data = new List<ProductDto>(), TotalCount = 0 };
        }

        public class GetProductsResponse
        {
            [JsonPropertyName("data")]
            public List<ProductDto> Data { get; set; } = new();

            public int TotalCount { get; set; }
            public bool Success { get; set; }
        }

        public async Task SaveProductAsync(ProductDto product)
        {
            await SetAuthHeader();

            product.UserId = await GetUserIdFromToken();

            var response = await _http.PostAsJsonAsync("api/Product/create", product);
        }

        private async Task SetAuthHeader()
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (string.IsNullOrEmpty(token))
            {
                token = await _js.InvokeAsync<string>("sessionStorage.getItem", "authToken");
            }
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<int> GetUserIdFromToken()
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (string.IsNullOrEmpty(token))
            {
                token = await _js.InvokeAsync<string>("sessionStorage.getItem", "authToken");
            }

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