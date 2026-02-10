using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace CreateInvoiceSystem.Frontend.Handler;

public class AuthenticationHeaderHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;

    public AuthenticationHeaderHandler(IJSRuntime js)
    {
        _js = js;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken")
                    ?? await _js.InvokeAsync<string>("sessionStorage.getItem", "authToken");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}