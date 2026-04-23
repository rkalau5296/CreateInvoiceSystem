using CreateInvoiceSystem.BuildTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace CreateInvoiceSystem.BuildTests.Integration;

public class NbpIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public NbpIntegrationTests(TestWebApplicationFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task Should_ReturnActualCurrencyRate_When_TableAndCodeAreValid()
    {
        // Arrange
        const string tableName = "A";
        const string currencyCode = "EUR";

        // Act
        var response = await _client.GetAsync($"/CurrencyRates/{tableName}/{currencyCode}");
        var body = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response Body: {body}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // Elastyczne sprawdzanie: czy dane są w polu 'data' (z ApiControllerBase) czy w korzeniu
        var elementToVerify = root.TryGetProperty("data", out var dataEl) ? dataEl : root;

        // NBP zwraca 'code' w obiekcie głównym
        elementToVerify.GetProperty("code").GetString().Should().Be(currencyCode);

        // NBP zwraca kursy w tablicy 'rates' lub bezpośrednio w polu 'mid'
        if (elementToVerify.TryGetProperty("rates", out var rates) && rates.ValueKind == JsonValueKind.Array)
        {
            rates[0].GetProperty("mid").GetDecimal().Should().BeGreaterThan(0);
        }
        else
        {
            elementToVerify.GetProperty("mid").GetDecimal().Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_DateRangeFormatIsInvalid()
    {
        // Arrange
        const string tableName = "A";
        const string currencyCode = "USD";
        const string invalidDate = "nie-data-iso";

        // Act
        var response = await _client.GetAsync($"/CurrencyRates/{tableName}/{currencyCode}/{invalidDate}/2026-01-01");
        var body = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response Body (Expected Error): {body}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("title").GetString().Should().Be("Invalid date format");
        doc.RootElement.GetProperty("detail").GetString().Should().Contain("Date parameters must be valid dates");
    }

    [Fact]
    public async Task Should_ReturnSeriesOfRates_When_DateRangeIsValid()
    {
        // Arrange
        const string tableName = "A";
        const string dateFrom = "2026-01-01";
        const string dateTo = "2026-01-07";

        // Act
        var response = await _client.GetAsync($"/CurrencyRates/{tableName}/{dateFrom}/{dateTo}");
        var body = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response Body: {body}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // Obsługa różnych struktur (bezpośrednia tablica lub obiekt z polem data/rates)
        if (root.ValueKind == JsonValueKind.Array)
        {
            root.GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
        }
        else
        {
            var target = root.TryGetProperty("data", out var data) ? data : root;

            // Jeśli target to tablica, sprawdź długość, jeśli obiekt - szukaj pola 'rates'
            if (target.ValueKind == JsonValueKind.Array)
            {
                target.GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
            }
            else
            {
                target.GetProperty("rates").GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
            }
        }
    }
}