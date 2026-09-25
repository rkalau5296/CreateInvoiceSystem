using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRates;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class GetSeriesCurrencyRatesFromToHandlerTests
{
    private readonly Mock<INbpApiRestService> _nbpApiRestServiceMock;
    private readonly IOptions<NbpApiOptions> _options;
    private readonly GetSeriesCurrencyRatesFromToHandler _sut;

    private const string BaseUrl = "http://api.nbp.pl/";

    public GetSeriesCurrencyRatesFromToHandlerTests()
    {
        _nbpApiRestServiceMock = new Mock<INbpApiRestService>();
        _options = Options.Create(new NbpApiOptions { BaseUrl = BaseUrl });

        _sut = new GetSeriesCurrencyRatesFromToHandler(_options, _nbpApiRestServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnListCurrencyRatesResponse_WhenRestServiceSucceeds()
    {
        // Arrange
        const string table = "A";
        var dateFrom = new DateTime(2026, 1, 1);
        var dateTo = new DateTime(2026, 1, 5);
        var request = new GetSeriesCurrencyRatesFromToRequest(table, dateFrom, dateTo);

        var expectedData = new List<CurrencyRatesTable>
        {
            new CurrencyRatesTable
            {
                Table = table,
                EffectiveDate = "2026-01-02",
                Rates = new List<CurrencyRate> { new CurrencyRate { Code = "USD", Mid = 4.0 } }
            },
            new CurrencyRatesTable
            {
                Table = table,
                EffectiveDate = "2026-01-05",
                Rates = new List<CurrencyRate> { new CurrencyRate { Code = "USD", Mid = 4.1 } }
            }
        };

        _nbpApiRestServiceMock
            .Setup(s => s.GetSeriesCurrencyRatesFromToAsync(
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
            s => s.GetSeriesCurrencyRatesFromToAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoDataFoundInDateRange()
    {
        // Arrange
        var dateFrom = new DateTime(2026, 1, 1);
        var dateTo = new DateTime(2026, 1, 2);
        var request = new GetSeriesCurrencyRatesFromToRequest("A", dateFrom, dateTo);

        _nbpApiRestServiceMock
            .Setup(s => s.GetSeriesCurrencyRatesFromToAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CurrencyRatesTable>());

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRestServiceFails()
    {
        // Arrange
        var request = new GetSeriesCurrencyRatesFromToRequest("C", DateTime.Now, DateTime.Now);

        _nbpApiRestServiceMock
            .Setup(s => s.GetSeriesCurrencyRatesFromToAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("NBP API is down"));

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("NBP API is down");
    }
}