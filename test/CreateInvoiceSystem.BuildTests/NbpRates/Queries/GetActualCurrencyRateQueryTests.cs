using Moq;
using FluentAssertions;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;

namespace CreateInvoiceSystem.BuildTests.NbpRates.Queries;

public class GetActualCurrencyRateQueryTests
{
    private readonly Mock<INbpApiRestService> _nbpApiRestServiceMock;

    public GetActualCurrencyRateQueryTests()
    {
        _nbpApiRestServiceMock = new Mock<INbpApiRestService>();
    }

    [Fact]
    public async Task Execute_ShouldCallRestServiceWithCorrectParameters()
    {
        // Arrange
        var table = "A";
        var currencyCode = "USD";
        var baseUrl = "http://api.nbp.pl/";
        var query = new GetActualCurrencyRateQuery(table, currencyCode, baseUrl);

        var expectedResult = new CurrencyRatesTable
        {
            Table = table,
            Code = currencyCode
        };

        _nbpApiRestServiceMock
            .Setup(s => s.GetActualCurrencyRateAsync(baseUrl, table, currencyCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await query.Execute(_nbpApiRestServiceMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Table.Should().Be(table);
        result.Code.Should().Be(currencyCode);

        _nbpApiRestServiceMock.Verify(s => s.GetActualCurrencyRateAsync(
            baseUrl,
            table,
            currencyCode,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldPropagateException_WhenRestServiceFails()
    {
        // Arrange
        var query = new GetActualCurrencyRateQuery("A", "EUR", "http://api.nbp.pl/");

        _nbpApiRestServiceMock
            .Setup(s => s.GetActualCurrencyRateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("404 Not Found"));

        // Act
        Func<Task> act = async () => await query.Execute(_nbpApiRestServiceMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("404 Not Found");
    }
}