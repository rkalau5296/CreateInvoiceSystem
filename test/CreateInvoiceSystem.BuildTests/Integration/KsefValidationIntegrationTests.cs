using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetIncomingKsefInvoices;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceDetails;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoicePdf;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceStatus;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceXml;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.KsefSettings;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.SendInvoiceToKsef;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CreateInvoiceSystem.BuildTests.Integration;

[Collection("Integration tests")]
public class KsefValidationIntegrationTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task MediatR_ValidatesEveryKsefRequestBeforeCallingHandlers()
    {
        var requests = new object[]
        {
            new GetIncomingKsefInvoicesRequest(),
            new GetKsefInvoiceDetailsRequest("number"),
            new GetKsefInvoicePdfRequest(0, "number"),
            new GetKsefInvoiceXmlRequest("number"),
            new GetKsefInvoiceStatusRequest(1),
            new SendInvoiceToKsefRequest(1),
            new GetKsefSettingsRequest(0),
            new SaveKsefSettingsRequest(0, "token"),
            new DeleteKsefSettingsRequest(0)
        };

        using var scope = fixture.Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        foreach (var request in requests)
        {
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => mediator.Send(request, TestContext.Current.CancellationToken));
            Assert.Contains(exception.Errors, error => error.PropertyName == "UserId");
        }
    }

    [Fact]
    public async Task InvalidRequests_ReturnValidationErrorsFromApi()
    {
        using var client = fixture.Factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;
        using var send = await client.PostAsync("/api/Ksef/0/send", null, ct);
        using var list = await client.GetAsync("/api/Ksef/invoices?pageSize=101", ct);
        using var settings = await client.PutAsJsonAsync("/api/Ksef/settings", new { Token = "bad token" }, ct);

        await AssertValidationError(send, "InvoiceId", ct);
        await AssertValidationError(list, "PageSize", ct);
        await AssertValidationError(settings, "Token", ct);
    }

    private static async Task AssertValidationError(HttpResponseMessage response, string property, CancellationToken ct)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        Assert.True(json.RootElement.GetProperty("errors").TryGetProperty(property, out _));
    }
}
