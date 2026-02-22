using CreateInvoiceSystem.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Json;

namespace CreateInvoiceSystem.Frontend.Services
{
    public class UserService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private readonly NavigationManager _navigationManager;
        private readonly AuthenticationStateProvider _authStateProvider;

        public UserService(
            HttpClient http,
            IJSRuntime js,
            NavigationManager navigationManager,
            AuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _js = js;
            _navigationManager = navigationManager;
            _authStateProvider = authStateProvider;
        }

        public async Task<GetUserResponse?> GetMe()
            => await _http.GetFromJsonAsync<GetUserResponse>("api/User/me");

        public async Task UpdateUser(int id, UpdateUserDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/User/update/{id}", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error: {error}");
            }
        }

        public async Task DeleteUserAccountAsync(int userId)
        {
            var response = await _http.DeleteAsync($"api/User/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            await _js.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
            await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _js.InvokeVoidAsync("sessionStorage.removeItem", "refreshToken");
            await _js.InvokeVoidAsync("localStorage.removeItem", "refreshToken");

            if (_authStateProvider is CustomAuthStateProvider customProvider)
            {
                customProvider.NotifyUserLogout();
            }

            _navigationManager.NavigateTo("/account-deleted");
        }
    }
}