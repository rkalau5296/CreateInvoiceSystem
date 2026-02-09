using CreateInvoiceSystem.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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
        private readonly CustomAuthStateProvider _authStateProvider;

        public AuthService(
            HttpClient httpClient,
            IJSRuntime jsRuntime,
            NavigationManager navigationManager,
            AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _navigationManager = navigationManager;
            _authStateProvider = (CustomAuthStateProvider)authStateProvider;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result != null && !string.IsNullOrEmpty(result.Token))
            {                
                string storageType = request.Dto.RememberMe ? "localStorage" : "sessionStorage";
                await _jsRuntime.InvokeVoidAsync($"{storageType}.setItem", "authToken", result.Token);
                await _jsRuntime.InvokeVoidAsync($"{storageType}.setItem", "refreshToken", result.RefreshToken);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);

                _authStateProvider.NotifyUserAuthentication(result.Token);
            }
            return result;
        }        

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _authStateProvider.NotifyUserLogout();
            _navigationManager.NavigateTo("/login");
        }

        public async Task<GetUserResponse?> GetMySettingsAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

                var request = new HttpRequestMessage(HttpMethod.Get, "api/User/me");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var userResponse = await response.Content.ReadFromJsonAsync<GetUserResponse>(options);
                    return userResponse;
                }

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

        public async Task<bool> RefreshTokenAsync()
        {
            var refreshToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "refreshToken");
            if (string.IsNullOrEmpty(refreshToken))
            {
                refreshToken = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "refreshToken");
            }

            if (string.IsNullOrEmpty(refreshToken) || !Guid.TryParse(refreshToken, out Guid refreshGuid))
            {
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;

            var response = await _httpClient.PostAsJsonAsync("api/User/refresh", refreshGuid);

            if (response.IsSuccessStatusCode)
            {
                var authData = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authData != null)
                {
                    var isLocal = !string.IsNullOrEmpty(await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken"));
                    var storage = isLocal ? "localStorage" : "sessionStorage";

                    await _jsRuntime.InvokeVoidAsync($"{storage}.setItem", "authToken", authData.Token);
                    await _jsRuntime.InvokeVoidAsync($"{storage}.setItem", "refreshToken", authData.RefreshToken);

                    return true;
                }
            }
            var errorDetails = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"REFRESH ERROR: Serwer odrzucił refresh. Status: {response.StatusCode}, Error: {errorDetails}");
            return false;
        }
    }
}