using CreateInvoiceSystem.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CreateInvoiceSystem.Frontend.Handler;

public class AuthenticationHeaderHandler : DelegatingHandler
{
    private readonly NavigationManager _navigationManager;
    private readonly IJSRuntime _jsRuntime;

    public AuthenticationHeaderHandler(NavigationManager navigationManager, IJSRuntime jsRuntime)
    {
        _navigationManager = navigationManager;
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        if (string.IsNullOrEmpty(token))
        {
            token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "authToken");
        }

        if (!string.IsNullOrEmpty(token))
        {            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshTokenStr = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "refreshToken");
            if (string.IsNullOrEmpty(refreshTokenStr))
            {
                refreshTokenStr = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "refreshToken");
            }

            if (Guid.TryParse(refreshTokenStr, out Guid refreshTokenGuid))
            {                
                var refreshResponse = await base.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/User/refresh")
                {
                    Content = JsonContent.Create(refreshTokenGuid)
                }, cancellationToken);

                if (refreshResponse.IsSuccessStatusCode)
                {
                    var authData = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);

                    if (authData != null && !string.IsNullOrEmpty(authData.Token))
                    {                        
                        string storage = !string.IsNullOrEmpty(await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken"))
                                         ? "localStorage" : "sessionStorage";

                        await _jsRuntime.InvokeVoidAsync($"{storage}.setItem", "authToken", authData.Token);
                        await _jsRuntime.InvokeVoidAsync($"{storage}.setItem", "refreshToken", authData.RefreshToken);
                        
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authData.Token);
                        
                        return await base.SendAsync(request, cancellationToken);
                    }
                }
            }
        }

        return response;
    }
}
