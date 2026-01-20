using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRates;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace CreateInvoiceSystem.BuildTests.NbpRates.Handlers;

public class GetActualCurrencyRatesHandlerTests
{
    private readonly Mock<IQueryExecutor> _queryExecutorMock;
    private readonly Mock<INbpApiRestService> _nbpApiRestServiceMock;
    private readonly IOptions<NbpApiOptions> _options;

    public GetActualCurrencyRatesHandlerTests()
    {
        _queryExecutorMock = new Mock<IQueryExecutor>();
        _nbpApiRestServiceMock = new Mock<INbpApiRestService>();

        var nbpOptions = new NbpApiOptions { BaseUrl = "http://api.nbp.pl/" };
        _options = Options.Create(nbpOptions);
    }

    [Fact]
    public async Task Handle_ShouldReturnCurrencyRatesResponse_WhenQueryIsSuccessful()
    {
        // Arrange
        var request = new GetActualCurrencyRatesRequest("A");
        var handler = new GetActualCurrencyRatesHandler(_queryExecutorMock.Object, _options, _nbpApiRestServiceMock.Object);

        var expectedData = new List<CurrencyRatesTable>
        {
            new CurrencyRatesTable
            {
                Table = "A",
                EffectiveDate = "2026-01-19",
                Rates = new List<CurrencyRate>
                {
                    new CurrencyRate { Code = "USD", Mid = 4.05 },
                    new CurrencyRate { Code = "EUR", Mid = 4.35 }
                }
            }
        };

        _queryExecutorMock.Setup(x => x.Execute(
                It.IsAny<GetActualCurrencyRatesQuery>(),
                _nbpApiRestServiceMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(expectedData);

        _queryExecutorMock.Verify(x => x.Execute(
            It.IsAny<GetActualCurrencyRatesQuery>(),
            _nbpApiRestServiceMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenQueryExecutorFails()
    {
        // Arrange
        var request = new GetActualCurrencyRatesRequest("B");
        var handler = new GetActualCurrencyRatesHandler(_queryExecutorMock.Object, _options, _nbpApiRestServiceMock.Object);

        _queryExecutorMock.Setup(x => x.Execute(
                It.IsAny<GetActualCurrencyRatesQuery>(),
                _nbpApiRestServiceMock.Object,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Network error");
    }
}