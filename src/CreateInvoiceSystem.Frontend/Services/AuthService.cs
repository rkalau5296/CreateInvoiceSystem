using CreateInvoiceSystem.Frontend.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

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
            
            return await _httpClient.GetFromJsonAsync<GetUserResponse>("api/User/me");
        }

        public async Task<bool> UpdateMySettingsAsync(UpdateUserDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/User/update/{dto.UserId}", dto);
            return response.IsSuccessStatusCode;
        }

    }
}