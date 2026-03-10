using CreateInvoiceSystem.Frontend.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace CreateInvoiceSystem.Frontend.Handler
{
    public class AuthenticatedAndRefreshedHandler(
        IServiceProvider _serviceProvider,
        IJSRuntime _js) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _js.InvokeAsync<string>("sessionStorage.getItem", "authToken");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var authService = _serviceProvider.GetRequiredService<AuthService>();
                var nav = _serviceProvider.GetRequiredService<NavigationManager>();
                var authStateProvider = _serviceProvider.GetService(typeof(AuthenticationStateProvider)) as AuthenticationStateProvider;

                string? serverMessage = null;
                try
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        using var doc = JsonDocument.Parse(content);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("detail", out var d) && d.ValueKind == JsonValueKind.String)
                            serverMessage = d.GetString();
                        else if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
                            serverMessage = t.GetString();
                    }
                }
                catch
                {
                }

                try
                {
                    var newToken = await authService.RefreshTokenAsync();

                    if (!string.IsNullOrEmpty(newToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                        response = await base.SendAsync(request, cancellationToken);
                    }
                    else
                    {
                        var msg = string.IsNullOrWhiteSpace(serverMessage) ? "Sesja wygasła" : serverMessage;

                        await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
                        await _js.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
                        await _js.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
                        await _js.InvokeVoidAsync("sessionStorage.removeItem", "refreshToken");

                        await _js.InvokeVoidAsync("sessionStorage.setItem", "sessionExpiredMessage", msg);

                        if (authStateProvider is CustomAuthStateProvider customProv)
                        {
                            customProv.NotifyUserLogout();
                        }

                        nav.NavigateTo("/login");
                    }
                }
                catch
                {
                    var msg = string.IsNullOrWhiteSpace(serverMessage) ? "Sesja wygasła" : serverMessage;

                    await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
                    await _js.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
                    await _js.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
                    await _js.InvokeVoidAsync("sessionStorage.removeItem", "refreshToken");

                    await _js.InvokeVoidAsync("sessionStorage.setItem", "sessionExpiredMessage", msg);

                    if (authStateProvider is CustomAuthStateProvider customProv)
                    {
                        customProv.NotifyUserLogout();
                    }

                    nav.NavigateTo("/login");
                }
            }

            return response;
        }
    }
}