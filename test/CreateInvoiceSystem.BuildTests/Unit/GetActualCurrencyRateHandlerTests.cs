using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRate;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class GetActualCurrencyRateHandlerTests
{
    private readonly Mock<INbpApiRestService> _nbpApiRestServiceMock;
    private readonly IOptions<NbpApiOptions> _options;
    private readonly GetActualCurrencyRateHandler _sut;

    private const string BaseUrl = "http://api.nbp.pl/";

    public GetActualCurrencyRateHandlerTests()
    {
        _nbpApiRestServiceMock = new Mock<INbpApiRestService>();
        _options = Options.Create(new NbpApiOptions { BaseUrl = BaseUrl });

        _sut = new GetActualCurrencyRateHandler(_options, _nbpApiRestServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCurrencyRateResponse_WhenRestServiceSucceeds()
    {
        // Arrange
        const string table = "A";
        const string currencyCode = "USD";
        var request = new GetActualCurrencyRateRequest(table, currencyCode);

        var expectedData = new CurrencyRatesTable
        {
            Table = table,
            Code = currencyCode,
            CurrencyCode = currencyCode,
            Rates = new List<CurrencyRate>
        {
            new CurrencyRate { Mid = 4.05, EffectiveDate = DateTime.Now }
        }
        };

        _nbpApiRestServiceMock
            .Setup(s => s.GetActualCurrencyRateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(expectedData);

        _nbpApiRestServiceMock.Verify(
            s => s.GetActualCurrencyRateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateHttpRequestException_WhenRestServiceFails()
    {
        // Arrange
        var request = new GetActualCurrencyRateRequest("A", "EUR");

        _nbpApiRestServiceMock
            .Setup(s => s.GetActualCurrencyRateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("404 Not Found"));

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("404 Not Found");
    }

    [Fact]
    public async Task Handle_ShouldPropagateGenericException_WhenRestServiceFails()
    {
        // Arrange
        var request = new GetActualCurrencyRateRequest("A", "EUR");

        _nbpApiRestServiceMock
            .Setup(s => s.GetActualCurrencyRateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API Error"));

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("API Error");
    }
}