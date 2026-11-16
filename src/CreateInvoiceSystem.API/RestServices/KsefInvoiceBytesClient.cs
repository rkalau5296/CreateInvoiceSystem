using System.Net.Http.Headers;
using KSeF.Client.Core.Infrastructure.Rest;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using KsefRouteBuilder = KSeF.Client.Core.Interfaces.Rest.IRouteBuilder;

namespace CreateInvoiceSystem.API.RestServices;

public sealed class KsefInvoiceBytesClient(HttpClient http, KsefRouteBuilder routes) : IKsefInvoiceBytesClient
{
    public async Task<byte[]> DownloadAsync(string ksefNumber, string accessToken, CancellationToken cancellationToken)
    {
        var path = routes.Build(Routes.Invoices.ByKsefNumber(Uri.EscapeDataString(ksefNumber)), null);
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
