using System.Net;
using System.Text;
using System.Text.Json;
using CreateInvoiceSystem.Frontend.Models;
using CreateInvoiceSystem.Frontend.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class FrontendJsonTests
{
    [Fact]
    public async Task Client_update_preserves_payload_shape()
    {
        string? body = null;
        var handler = new StubHandler(async request =>
        {
            body = await request.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var service = new ClientService(new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") },
            Mock.Of<IJSRuntime>());

        await service.UpdateClientAsync(new ClientDto
        {
            ClientId = 7,
            Name = "Example",
            Nip = "1234567890",
            Email = "client@example.test",
            Address = new AddressDto { Street = "Street", Number = "1", City = "City", PostalCode = "00-000", Country = "PL" }
        });

        using var json = JsonDocument.Parse(Assert.IsType<string>(body));
        Assert.Equal(5, json.RootElement.EnumerateObject().Count());
        Assert.Equal(7, json.RootElement.GetProperty("clientId").GetInt32());
        var address = json.RootElement.GetProperty("address");
        Assert.Equal(5, address.EnumerateObject().Count());
        Assert.Equal("Street", address.GetProperty("street").GetString());
    }

    [Fact]
    public async Task Product_update_preserves_null_price()
    {
        string? body = null;
        var handler = new StubHandler(async request =>
        {
            body = await request.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var service = new ProductService(new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") },
            Mock.Of<IJSRuntime>(), Mock.Of<NavigationManager>());

        await service.UpdateProductAsync(new ProductDto { ProductId = 3, Name = "Product", Value = null });

        using var json = JsonDocument.Parse(Assert.IsType<string>(body));
        Assert.Equal(4, json.RootElement.EnumerateObject().Count());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("value").ValueKind);
    }

    [Fact]
    public async Task Ksef_settings_accepts_case_insensitive_response()
    {
        var handler = new StubHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"IsConfigured\":true}", Encoding.UTF8, "application/json")
        }));
        var service = new KsefService(new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") },
            Mock.Of<IJSRuntime>());

        Assert.True(await service.IsConfiguredAsync());
    }

    private sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request);
    }
}
