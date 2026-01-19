using Moq;
using FluentAssertions;
using Xunit;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;

namespace CreateInvoiceSystem.BuildTests.NbpRates.Queries;

public class GetSeriesCurrencyRatesFromToQueryTests
{
    private readonly Mock<INbpApiRestService> _nbpApiRestServiceMock;

    public GetSeriesCurrencyRatesFromToQueryTests()
    {
        _nbpApiRestServiceMock = new Mock<INbpApiRestService>();
    }

    [Fact]
    public async Task Execute_ShouldCallRestServiceWithCorrectParameters()
    {
        // Arrange
        var table = "A";
        var dateFrom = new DateTime(2026, 1, 1);
        var dateTo = new DateTime(2026, 1, 15);
        var baseUrl = "http://api.nbp.pl/";

        var query = new GetSeriesCurrencyRatesFromToQuery(table, dateFrom, dateTo, baseUrl);

        var expectedResult = new List<CurrencyRatesTable>
        {
            new CurrencyRatesTable { Table = "A", EffectiveDate = "2026-01-05" },
            new CurrencyRatesTable { Table = "A", EffectiveDate = "2026-01-12" }
        };

        _nbpApiRestServiceMock
            .Setup(s => s.GetSeriesCurrencyRatesFromToAsync(baseUrl, table, dateFrom, dateTo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await query.Execute(_nbpApiRestServiceMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Table.Should().Be(table);

        _nbpApiRestServiceMock.Verify(s => s.GetSeriesCurrencyRatesFromToAsync(
            baseUrl,
            table,
            dateFrom,
            dateTo,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldReturnEmptyList_WhenNoDataFoundInDateRange()
    {
        // Arrange
        var dateFrom = new DateTime(2026, 1, 1);
        var dateTo = new DateTime(2026, 1, 2);
        var query = new GetSeriesCurrencyRatesFromToQuery("A", dateFrom, dateTo, "http://api.nbp.pl/");

        _nbpApiRestServiceMock
            .Setup(s => s.GetSeriesCurrencyRatesFromToAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CurrencyRatesTable>());

        // Act
        var result = await query.Execute(_nbpApiRestServiceMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_ShouldPropagateException_WhenRestServiceFails()
    {
        // Arrange
        var query = new GetSeriesCurrencyRatesFromToQuery("A", DateTime.Now, DateTime.Now, "http://api.nbp.pl/");

        _nbpApiRestServiceMock
            .Setup(s => s.GetSeriesCurrencyRatesFromToAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("NBP API is down"));

        // Act
        Func<Task> act = async () => await query.Execute(_nbpApiRestServiceMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("NBP API is down");
    }
}