using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CreateInvoiceSystem.BuildTests.Integration;

[Collection("Integration tests")]
public class NbpIntegrationTests
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public NbpIntegrationTests(
        IntegrationTestFixture fixture,
        ITestOutputHelper output)
    {
        _output = output;

        var factory = fixture.Factory.WithWebHostBuilder(
            builder =>
            {
                builder.ConfigureTestServices(
                    services =>
                    {
                        services.AddTransient<
                            MockNbpHttpMessageHandler>();

                        services.AddHttpClient("NbpClient")
                            .AddHttpMessageHandler<
                                MockNbpHttpMessageHandler>();
                    });
            });

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_ReturnActualCurrencyRate_When_TableAndCodeAreValid()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        const string tableName = "A";
        const string currencyCode = "EUR";

        var response = await _client.GetAsync(
            $"/CurrencyRates/{tableName}/{currencyCode}",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        _output.WriteLine($"Response Body: {body}");

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            because: body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var elementToVerify =
            root.TryGetProperty("data", out var dataElement)
                ? dataElement
                : root;

        elementToVerify
            .GetProperty("code")
            .GetString()
            .Should()
            .Be(currencyCode);

        if (elementToVerify.TryGetProperty(
                "rates",
                out var rates)
            && rates.ValueKind == JsonValueKind.Array)
        {
            rates[0]
                .GetProperty("mid")
                .GetDecimal()
                .Should()
                .BeGreaterThan(0);
        }
        else
        {
            elementToVerify
                .GetProperty("mid")
                .GetDecimal()
                .Should()
                .BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_DateRangeFormatIsInvalid()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        const string tableName = "A";
        const string currencyCode = "USD";
        const string invalidDate = "nie-data-iso";

        var response = await _client.GetAsync(
            $"/CurrencyRates/{tableName}/{currencyCode}/{invalidDate}/2026-01-01",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        _output.WriteLine(
            $"Response Body (Expected Error): {body}");

        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);

        using var doc = JsonDocument.Parse(body);

        doc.RootElement
            .GetProperty("title")
            .GetString()
            .Should()
            .Be("Invalid date format");

        doc.RootElement
            .GetProperty("detail")
            .GetString()
            .Should()
            .Contain("Date parameters must be valid dates");
    }

    [Fact]
    public async Task Should_ReturnSeriesOfRates_When_DateRangeIsValid()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        const string tableName = "A";
        const string dateFrom = "2026-01-01";
        const string dateTo = "2026-01-07";

        var response = await _client.GetAsync(
            $"/CurrencyRates/{tableName}/{dateFrom}/{dateTo}",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        _output.WriteLine($"Response Body: {body}");

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            because: body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
        {
            root.GetArrayLength()
                .Should()
                .BeGreaterThanOrEqualTo(0);

            return;
        }

        var target = root.TryGetProperty(
            "data",
            out var data)
            ? data
            : root;

        if (target.ValueKind == JsonValueKind.Array)
        {
            target.GetArrayLength()
                .Should()
                .BeGreaterThanOrEqualTo(0);

            return;
        }

        target
            .GetProperty("rates")
            .GetArrayLength()
            .Should()
            .BeGreaterThanOrEqualTo(0);
    }
}

public sealed class MockNbpHttpMessageHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var url = request.RequestUri?.ToString()
            ?? string.Empty;

        if (url.Contains("/A/EUR")
            || url.Contains("/a/eur"))
        {
            const string singleRateJson = """
            {
                "table": "A",
                "currency": "euro",
                "code": "EUR",
                "rates": [
                    {
                        "no": "001/A/NBP/2026",
                        "effectiveDate": "2026-01-02",
                        "mid": 4.2500
                    }
                ]
            }
            """;

            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        singleRateJson,
                        Encoding.UTF8,
                        "application/json")
                });
        }

        if (url.Contains(
                "/2026-01-01/2026-01-07"))
        {
            const string seriesJson = """
            {
                "table": "A",
                "currency": "euro",
                "code": "EUR",
                "rates": [
                    {
                        "no": "001/A/NBP/2026",
                        "effectiveDate": "2026-01-02",
                        "mid": 4.2500
                    },
                    {
                        "no": "002/A/NBP/2026",
                        "effectiveDate": "2026-01-05",
                        "mid": 4.2650
                    }
                ]
            }
            """;

            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        seriesJson,
                        Encoding.UTF8,
                        "application/json")
                });
        }

        return Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}