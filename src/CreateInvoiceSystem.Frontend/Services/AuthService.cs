using CreateInvoiceSystem.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Json;

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
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, FrontendJsonContext.Default.LoginRequest);

            await response.EnsureSuccessOrThrowApiExceptionAsync();

            var result = await response.Content.ReadFromJsonAsync(FrontendJsonContext.Default.LoginResponse);
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

                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "sessionExpiredMessage");

                _authStateProvider.NotifyUserAuthentication(result.Token);
            }
            return result;
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");

            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "refreshToken");

            _authStateProvider.NotifyUserLogout();
            _navigationManager.NavigateTo("/login");
        }

        public async Task<string?> RefreshTokenAsync()
        {
            var localToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "refreshToken");
            var sessionToken = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "refreshToken");

            string storageType = !string.IsNullOrEmpty(localToken) ? "localStorage" : "sessionStorage";
            string? currentRefreshToken = !string.IsNullOrEmpty(localToken) ? localToken : sessionToken;

            if (string.IsNullOrEmpty(currentRefreshToken)) return null;

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/User/refresh", Guid.Parse(currentRefreshToken), FrontendJsonContext.Default.Guid);
                await response.EnsureSuccessOrThrowApiExceptionAsync();

                var authData = await response.Content.ReadFromJsonAsync(FrontendJsonContext.Default.AuthResponse);
                if (authData != null && !string.IsNullOrEmpty(authData.Token))
                {
                    await _jsRuntime.InvokeVoidAsync($"{storageType}.setItem", "authToken", authData.Token);
                    await _jsRuntime.InvokeVoidAsync($"{storageType}.setItem", "refreshToken", authData.RefreshToken);

                    return authData.Token;
                }

                return null;
            }
            catch (ApiException aex) when (aex.StatusCode == 401)
            {
                var msg = aex.GetUserMessage() ?? "Sesja wygasła";

                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "refreshToken");

                _authStateProvider.NotifyUserLogout();

                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "sessionExpiredMessage", msg);

                return null;
            }
        }

        public async Task<ForgotPasswordResponse> ForgotPasswordAsync(string email)
        {
            var dto = new ForgotPasswordDto(email);
            var request = new ForgotPasswordRequest(dto);

            var response = await _httpClient.PostAsJsonAsync("api/Auth/forgot-password", request, FrontendJsonContext.Default.ForgotPasswordRequest);
            await response.EnsureSuccessOrThrowApiExceptionAsync();

            return await response.Content.ReadFromJsonAsync(FrontendJsonContext.Default.ForgotPasswordResponse)
                   ?? new ForgotPasswordResponse(false, "Błąd odpowiedzi.");
        }

        public async Task<ResetPasswordResponse> ResetPasswordAsync(string email, string token, string version, string newPassword)
        {
            var request = new ResetPasswordPayload(email, token, version, newPassword);
            var response = await _httpClient.PostAsJsonAsync("api/Auth/reset-password", request, FrontendJsonContext.Default.ResetPasswordPayload);
            await response.EnsureSuccessOrThrowApiExceptionAsync();

            return await response.Content.ReadFromJsonAsync(FrontendJsonContext.Default.ResetPasswordResponse)
                   ?? new ResetPasswordResponse(false, "Błąd odpowiedzi.");
        }

        public async Task<RegisterUserResponse?> RegisterAsync(RegisterUserRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", request, FrontendJsonContext.Default.RegisterUserRequest);
            await response.EnsureSuccessOrThrowApiExceptionAsync();
            return await response.Content.ReadFromJsonAsync(FrontendJsonContext.Default.RegisterUserResponse);
        }

        public sealed record ResetPasswordPayload(string Email, string Token, string Version, string NewPassword);
    }
}