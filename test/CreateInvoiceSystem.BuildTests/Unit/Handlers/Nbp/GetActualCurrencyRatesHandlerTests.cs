using CreateInvoiceSystem.Modules.Nbp.Application.DTO;
using CreateInvoiceSystem.Modules.Nbp.Application.Handlers;
using CreateInvoiceSystem.Modules.Nbp.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.ActualRates;
using CreateInvoiceSystem.Modules.Nbp.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Nbp;

public class GetActualCurrencyRatesHandlerTests
{
    private readonly Mock<INbpApiRestService> _nbpApiRestServiceMock;
    private readonly IOptions<NbpApiOptions> _options;
    private readonly GetActualCurrencyRatesHandler _sut;

    private const string BaseUrl = "http://api.nbp.pl/";

    public GetActualCurrencyRatesHandlerTests()
    {
        _nbpApiRestServiceMock = new Mock<INbpApiRestService>();
        _options = Options.Create(new NbpApiOptions { BaseUrl = BaseUrl });

        _sut = new GetActualCurrencyRatesHandler(_nbpApiRestServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCurrencyRatesResponse_WhenRestServiceSucceeds()
    {
        // Arrange
        const string table = "A";
        var request = new GetActualCurrencyRatesRequest(table);

        var expectedData = new List<CurrencyRatesTable>
        {
            new CurrencyRatesTable
            {
                Table = table,
                EffectiveDate = "2026-01-19",
                Rates = new List<CurrencyRate>
                {
                    new CurrencyRate { Code = "USD", Mid = 4.05 },
                    new CurrencyRate { Code = "EUR", Mid = 4.35 }
                }
            }
        };

        _nbpApiRestServiceMock
            .Setup(s => s.GetActualCurrencyRatesAsync(                
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(expectedData);

        _nbpApiRestServiceMock.Verify(
            s => s.GetActualCurrencyRatesAsync(                
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenRestServiceReturnsEmpty()
    {
        // Arrange
        var request = new GetActualCurrencyRatesRequest("B");

        _nbpApiRestServiceMock
            .Setup(s => s.GetActualCurrencyRatesAsync(                
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CurrencyRatesTable>());

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRestServiceThrows()
    {
        // Arrange
        var request = new GetActualCurrencyRatesRequest("A");

        _nbpApiRestServiceMock
            .Setup(s => s.GetActualCurrencyRatesAsync(               
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Network error");
    }
}