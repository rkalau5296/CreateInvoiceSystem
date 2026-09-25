using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRate;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class GetSeriesCurrencyRateFromToHandlerTests
{
    private readonly Mock<INbpApiRestService> _nbpApiRestServiceMock;
    private readonly IOptions<NbpApiOptions> _options;
    private readonly GetSeriesCurrencyRateFromToHandler _sut;

    private const string BaseUrl = "http://api.nbp.pl/";

    public GetSeriesCurrencyRateFromToHandlerTests()
    {
        _nbpApiRestServiceMock = new Mock<INbpApiRestService>();
        _options = Options.Create(new NbpApiOptions { BaseUrl = BaseUrl });

        _sut = new GetSeriesCurrencyRateFromToHandler(_options, _nbpApiRestServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSeriesCurrencyRateResponse_WhenRestServiceSucceeds()
    {
        // Arrange
        const string table = "A";
        const string currencyCode = "USD";
        var dateFrom = new DateTime(2026, 1, 1);
        var dateTo = new DateTime(2026, 1, 10);
        var request = new GetSeriesCurrencyRateFromToRequest(table, currencyCode, dateFrom, dateTo);

        var expectedData = new CurrencyRatesTable
        {
            Table = table,
            Code = currencyCode,
            Rates = new List<CurrencyRate>
            {
                new CurrencyRate { Mid = 4.05, EffectiveDate = dateFrom },
                new CurrencyRate { Mid = 4.10, EffectiveDate = dateTo }
            }
        };

        _nbpApiRestServiceMock
            .Setup(s => s.GetSeriesCurrencyRateFromToAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(expectedData);

        _nbpApiRestServiceMock.Verify(
            s => s.GetSeriesCurrencyRateFromToAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRestServiceFails()
    {
        // Arrange
        var dateFrom = DateTime.Now.AddDays(-7);
        var dateTo = DateTime.Now;
        var request = new GetSeriesCurrencyRateFromToRequest("A", "EUR", dateFrom, dateTo);

        _nbpApiRestServiceMock
            .Setup(s => s.GetSeriesCurrencyRateFromToAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid date range"));

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Invalid date range");
    }
}