using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRates;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace CreateInvoiceSystem.BuildTests.NbpRates.Handlers;

public class GetSeriesCurrencyRatesFromToHandlerTests
{
    private readonly Mock<IQueryExecutor> _queryExecutorMock;
    private readonly Mock<INbpApiRestService> _nbpApiRestServiceMock;
    private readonly IOptions<NbpApiOptions> _options;

    public GetSeriesCurrencyRatesFromToHandlerTests()
    {
        _queryExecutorMock = new Mock<IQueryExecutor>();
        _nbpApiRestServiceMock = new Mock<INbpApiRestService>();

        var nbpOptions = new NbpApiOptions { BaseUrl = "http://api.nbp.pl/" };
        _options = Options.Create(nbpOptions);
    }

    [Fact]
    public async Task Handle_ShouldReturnListCurrencyRatesResponse_WhenQueryIsSuccessful()
    {
        // Arrange
        var dateFrom = new DateTime(2026, 1, 1);
        var dateTo = new DateTime(2026, 1, 5);
        var request = new GetSeriesCurrencyRatesFromToRequest("A", dateFrom, dateTo);
        var handler = new GetSeriesCurrencyRatesFromToHandler(_queryExecutorMock.Object, _options, _nbpApiRestServiceMock.Object);

        var expectedData = new List<CurrencyRatesTable>
        {
            new CurrencyRatesTable
            {
                Table = "A",
                EffectiveDate = "2026-01-02",
                Rates = new List<CurrencyRate> { new CurrencyRate { Code = "USD", Mid = 4.0 } }
            },
            new CurrencyRatesTable
            {
                Table = "A",
                EffectiveDate = "2026-01-05",
                Rates = new List<CurrencyRate> { new CurrencyRate { Code = "USD", Mid = 4.1 } }
            }
        };

        _queryExecutorMock.Setup(x => x.Execute(
                It.IsAny<GetSeriesCurrencyRatesFromToQuery>(),
                _nbpApiRestServiceMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(expectedData);

        _queryExecutorMock.Verify(x => x.Execute(
            It.IsAny<GetSeriesCurrencyRatesFromToQuery>(),
            _nbpApiRestServiceMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenQueryExecutorFails()
    {
        // Arrange
        var request = new GetSeriesCurrencyRatesFromToRequest("C", DateTime.Now, DateTime.Now);
        var handler = new GetSeriesCurrencyRatesFromToHandler(_queryExecutorMock.Object, _options, _nbpApiRestServiceMock.Object);

        _queryExecutorMock.Setup(x => x.Execute(
                It.IsAny<GetSeriesCurrencyRatesFromToQuery>(),
                _nbpApiRestServiceMock.Object,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid table"));

        // Act
        Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Invalid table");
    }
}