using Moq;
using FluentAssertions;
using Xunit;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;

namespace CreateInvoiceSystem.BuildTests.NbpRates.Queries;

public class GetSeriesCurrencyRateFromToQueryTests
{
    private readonly Mock<INbpApiRestService> _nbpApiRestServiceMock;

    public GetSeriesCurrencyRateFromToQueryTests()
    {
        _nbpApiRestServiceMock = new Mock<INbpApiRestService>();
    }

    [Fact]
    public async Task Execute_ShouldCallRestServiceWithCorrectParameters()
    {
        // Arrange
        var table = "A";
        var currencyCode = "USD";
        var dateFrom = new DateTime(2026, 1, 1);
        var dateTo = new DateTime(2026, 1, 10);
        var baseUrl = "http://api.nbp.pl/";

        var query = new GetSeriesCurrencyRateFromToQuery(table, currencyCode, dateFrom, dateTo, baseUrl);

        var expectedResult = new CurrencyRatesTable
        {
            Table = table,
            Code = currencyCode,
            Rates = new List<CurrencyRate>()
        };

        _nbpApiRestServiceMock
            .Setup(s => s.GetSeriesCurrencyRateFromToAsync(baseUrl, table, currencyCode, dateFrom, dateTo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await query.Execute(_nbpApiRestServiceMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be(currencyCode);

        _nbpApiRestServiceMock.Verify(s => s.GetSeriesCurrencyRateFromToAsync(
            baseUrl,
            table,
            currencyCode,
            dateFrom,
            dateTo,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldPropagateException_WhenRestServiceFails()
    {
        // Arrange
        var dateFrom = DateTime.Now.AddDays(-7);
        var dateTo = DateTime.Now;
        var query = new GetSeriesCurrencyRateFromToQuery("A", "EUR", dateFrom, dateTo, "http://api.nbp.pl/");

        _nbpApiRestServiceMock
            .Setup(s => s.GetSeriesCurrencyRateFromToAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid date range"));

        // Act
        Func<Task> act = async () => await query.Execute(_nbpApiRestServiceMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Invalid date range");
    }
}