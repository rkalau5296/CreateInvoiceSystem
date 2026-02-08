using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;

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

        if (!string.IsNullOrEmpty(token))
        {     
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _navigationManager.NavigateTo("/login");
        }

        return response;
    }
}
