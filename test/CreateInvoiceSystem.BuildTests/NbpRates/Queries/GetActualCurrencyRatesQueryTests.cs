using Moq;
using FluentAssertions;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;

namespace CreateInvoiceSystem.BuildTests.NbpRates.Queries;

public class GetActualCurrencyRatesQueryTests
{
    private readonly Mock<INbpApiRestService> _nbpApiRestServiceMock;

    public GetActualCurrencyRatesQueryTests()
    {
        _nbpApiRestServiceMock = new Mock<INbpApiRestService>();
    }

    [Fact]
    public async Task Execute_ShouldCallRestServiceWithCorrectParameters()
    {
        // Arrange
        var table = "A";
        var baseUrl = "http://api.nbp.pl/";
        var query = new GetActualCurrencyRatesQuery(table, baseUrl);

        var expectedResult = new List<CurrencyRatesTable>
        {
            new CurrencyRatesTable { Table = "A", EffectiveDate = "2026-01-19" }
        };

        _nbpApiRestServiceMock
            .Setup(s => s.GetActualCurrencyRatesAsync(baseUrl, table, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await query.Execute(_nbpApiRestServiceMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Table.Should().Be(table);

        _nbpApiRestServiceMock.Verify(s => s.GetActualCurrencyRatesAsync(
            baseUrl,
            table,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldReturnEmptyList_WhenRestServiceReturnsEmpty()
    {
        // Arrange
        var query = new GetActualCurrencyRatesQuery("B", "http://api.nbp.pl/");

        _nbpApiRestServiceMock
            .Setup(s => s.GetActualCurrencyRatesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CurrencyRatesTable>());

        // Act
        var result = await query.Execute(_nbpApiRestServiceMock.Object, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_ShouldPropagateException_WhenRestServiceThrows()
    {
        // Arrange
        var query = new GetActualCurrencyRatesQuery("A", "http://api.nbp.pl/");

        _nbpApiRestServiceMock
            .Setup(s => s.GetActualCurrencyRatesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));

        // Act
        Func<Task> act = async () => await query.Execute(_nbpApiRestServiceMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Connection failed");
    }
}