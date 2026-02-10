using CreateInvoiceSystem.Frontend.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;

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
                        nav.NavigateTo("/login");
                    }
                }
                catch
                {                    
                    nav.NavigateTo("/login");
                }
            }

            return response;
        }
    }
}