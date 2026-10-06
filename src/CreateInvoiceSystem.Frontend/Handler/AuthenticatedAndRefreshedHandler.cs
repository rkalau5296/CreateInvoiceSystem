using CreateInvoiceSystem.Frontend.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CreateInvoiceSystem.Frontend.Handler
{
    public class AuthenticatedAndRefreshedHandler(
        IServiceProvider _serviceProvider,
        IJSRuntime _js) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            byte[]? body = null;
            if (request.Content is not null)
            {
                body = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            }

            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }

            var serverMessage = await ReadServerMessageAsync(response, cancellationToken);

            string? newToken = null;
            try
            {
                var authService = _serviceProvider.GetRequiredService<AuthService>();
                newToken = await authService.RefreshTokenAsync();
            }
            catch
            {
            }

            if (!string.IsNullOrEmpty(newToken))
            {
                response.Dispose();

                var retry = CloneRequest(request, body);
                retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                return await base.SendAsync(retry, cancellationToken);
            }

            await ExpireSessionAsync(serverMessage);
            return response;
        }

        private async Task<string?> GetTokenAsync()
        {
            var localToken = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(localToken))
            {
                return localToken;
            }

            return await _js.InvokeAsync<string>("sessionStorage.getItem", "authToken");
        }

        private static async Task<string?> ReadServerMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return null;
                }

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (root.TryGetProperty("detail", out var d) && d.ValueKind == JsonValueKind.String)
                    return d.GetString();
                if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
                    return t.GetString();
            }
            catch
            {
            }

            return null;
        }

        private static HttpRequestMessage CloneRequest(HttpRequestMessage original, byte[]? body)
        {
            var clone = new HttpRequestMessage(original.Method, original.RequestUri)
            {
                Version = original.Version
            };

            foreach (var header in original.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (body is not null)
            {
                var content = new ByteArrayContent(body);
                foreach (var header in original.Content!.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
                clone.Content = content;
            }

            return clone;
        }

        private async Task ExpireSessionAsync(string? serverMessage)
        {
            var msg = string.IsNullOrWhiteSpace(serverMessage) ? "Sesja wygasła" : serverMessage;

            await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _js.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
            await _js.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
            await _js.InvokeVoidAsync("sessionStorage.removeItem", "refreshToken");

            await _js.InvokeVoidAsync("sessionStorage.setItem", "sessionExpiredMessage", msg);

            if (_serviceProvider.GetService<AuthenticationStateProvider>() is CustomAuthStateProvider customProv)
            {
                customProv.NotifyUserLogout();
            }

            _serviceProvider.GetRequiredService<NavigationManager>().NavigateTo("/login");
        }
    }
}