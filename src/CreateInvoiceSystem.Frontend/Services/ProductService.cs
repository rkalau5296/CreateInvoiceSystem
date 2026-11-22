using CreateInvoiceSystem.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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
                return await response.Content.ReadFromJsonAsync(FrontendJsonContext.Default.GetProductsResponse) ?? new GetProductsResponse();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _navigationManager.NavigateTo("/login");
                return new GetProductsResponse();
            }

            await response.EnsureSuccessOrThrowApiExceptionAsync();
            return new GetProductsResponse();
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            const int pageSize = 100;
            const int maxPages = 100;

            var all = new List<ProductDto>();
            for (var page = 1; page <= maxPages; page++)
            {
                var response = await GetProductsAsync(page, pageSize);
                var items = response.Data;
                if (items.Count == 0) break;

                all.AddRange(items);
                if (all.Count >= response.TotalCount) break;
            }

            return all;
        }

        public async Task SaveProductAsync(ProductDto product)
        {            
            var response = await _http.PostAsJsonAsync("api/Product/create", product, FrontendJsonContext.Default.ProductDto);
            await response.EnsureSuccessOrThrowApiExceptionAsync();
        }

        public async Task UpdateProductAsync(ProductDto product)
        {
            var updateDto = new UpdateProductPayload(product.ProductId, product.Name, product.Description, product.Value);

            var response = await _http.PutAsJsonAsync($"api/Product/update/{product.ProductId}", updateDto, FrontendJsonContext.Default.UpdateProductPayload);
            await response.EnsureSuccessOrThrowApiExceptionAsync();
        }

        public sealed record UpdateProductPayload(int ProductId, string Name, string Description, decimal? Value);

        public async Task DeleteProductAsync(int productId)
        {
            var response = await _http.DeleteAsync($"api/Product/{productId}");
            await response.EnsureSuccessOrThrowApiExceptionAsync();
        }

        public async Task DownloadProductsCsvAsync()
        {
            var response = await _http.GetAsync("api/export/products");

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                await _js.InvokeVoidAsync("downloadFile", "produkty.csv", "text/csv", fileBytes);
            }
            else
            {
                await response.EnsureSuccessOrThrowApiExceptionAsync();
            }
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