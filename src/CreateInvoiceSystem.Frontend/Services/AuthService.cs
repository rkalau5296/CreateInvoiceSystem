using CreateInvoiceSystem.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CreateInvoiceSystem.Frontend.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigationManager;

        public AuthService(HttpClient httpClient, IJSRuntime jsRuntime, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _navigationManager = navigationManager;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponse>();
            }

            return null;
        }

        public async Task LogoutAsync()
        {            
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            _navigationManager.NavigateTo("/login", forceLoad: true);
        }

        public async Task<GetUserResponse?> GetMySettingsAsync()
        {
            try
            {
                // 1. Pobieramy token bezpośrednio
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

                // 2. Tworzymy zapytanie ręcznie
                var request = new HttpRequestMessage(HttpMethod.Get, "api/User/me");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // 3. Wysyłamy
                var response = await _httpClient.SendAsync(request);
                

                if (response.IsSuccessStatusCode)
                {
                    var userResponse = await response.Content.ReadFromJsonAsync<GetUserResponse>();
                    return userResponse;
                }

                // Log błędu do konsoli F12
                var errorContent = await response.Content.ReadAsStringAsync();
                await _jsRuntime.InvokeVoidAsync("console.error", $"API zwróciło błąd {response.StatusCode}: {errorContent}");

                return null;
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", "Wyjątek w GetMySettingsAsync: " + ex.Message);
                return null;
            }
        }

        public async Task<bool> UpdateMySettingsAsync(UpdateUserDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/User/update/{dto.UserId}", dto);
            return response.IsSuccessStatusCode;
        }
    }
}