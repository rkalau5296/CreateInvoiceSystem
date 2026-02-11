using CreateInvoiceSystem.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

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

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result is { IsSuccess: true } && !string.IsNullOrEmpty(result.Token))
            {
                string storageType = request.Dto.RememberMe ? "localStorage" : "sessionStorage";

                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "refreshToken");

                await _jsRuntime.InvokeVoidAsync($"{storageType}.setItem", "authToken", result.Token);
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    await _jsRuntime.InvokeVoidAsync($"{storageType}.setItem", "refreshToken", result.RefreshToken);
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
            }
            return result;
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");

            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "refreshToken");

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

        public async Task<string?> RefreshTokenAsync()
        {            
            var localToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "refreshToken");
            var sessionToken = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "refreshToken");

            string storageType = !string.IsNullOrEmpty(localToken) ? "localStorage" : "sessionStorage";
            string? currentRefreshToken = !string.IsNullOrEmpty(localToken) ? localToken : sessionToken;

            if (string.IsNullOrEmpty(currentRefreshToken)) return null;

            var response = await _httpClient.PostAsJsonAsync("api/User/refresh", Guid.Parse(currentRefreshToken));

            if (response.IsSuccessStatusCode)
            {
                var authData = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authData != null && !string.IsNullOrEmpty(authData.Token))
                {                    
                    await _jsRuntime.InvokeVoidAsync($"{storageType}.setItem", "authToken", authData.Token);
                    await _jsRuntime.InvokeVoidAsync($"{storageType}.setItem", "refreshToken", authData.RefreshToken);

                    return authData.Token;
                }
            }
            return null;
        }
        public async Task<ForgotPasswordResponse> ForgotPasswordAsync(string email)
        {
            var dto = new ForgotPasswordDto(email);
            var request = new ForgotPasswordRequest(dto);

            var response = await _httpClient.PostAsJsonAsync("api/Auth/forgot-password", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
            }

            return new ForgotPasswordResponse(false, "Błąd serwera. Spróbuj później.");
        }

        public async Task<ResetPasswordResponse> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var request = new { Email = email, Token = token, NewPassword = newPassword };
            var response = await _httpClient.PostAsJsonAsync("api/Auth/reset-password", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ResetPasswordResponse>()
                       ?? new ResetPasswordResponse(false, "Błąd odpowiedzi.");
            }

            return new ResetPasswordResponse(false, "Nie udało się zresetować hasła.");
        }
    }
}