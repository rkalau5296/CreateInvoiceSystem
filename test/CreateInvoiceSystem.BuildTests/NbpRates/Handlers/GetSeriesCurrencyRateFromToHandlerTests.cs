using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRate;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRate;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.NbpRates.Handlers;

public class GetSeriesCurrencyRateFromToHandlerTests
{
    private readonly Mock<IQueryExecutor> _queryExecutorMock;
    private readonly Mock<INbpApiRestService> _nbpApiRestServiceMock;
    private readonly IOptions<NbpApiOptions> _options;

    public GetSeriesCurrencyRateFromToHandlerTests()
    {
        _queryExecutorMock = new Mock<IQueryExecutor>();
        _nbpApiRestServiceMock = new Mock<INbpApiRestService>();

        var nbpOptions = new NbpApiOptions { BaseUrl = "http://api.nbp.pl/" };
        _options = Options.Create(nbpOptions);
    }

    [Fact]
    public async Task Handle_ShouldReturnSeriesCurrencyRateResponse_WhenQueryIsSuccessful()
    {
        // Arrange
        var dateFrom = new DateTime(2026, 1, 1);
        var dateTo = new DateTime(2026, 1, 10);
        var request = new GetSeriesCurrencyRateFromToRequest("A", "USD", dateFrom, dateTo);
        var handler = new GetSeriesCurrencyRateFromToHandler(_queryExecutorMock.Object, _options, _nbpApiRestServiceMock.Object);

        var expectedData = new CurrencyRatesTable
        {
            Table = "A",
            Code = "USD",
            Rates = new List<CurrencyRate>
            {
                new CurrencyRate { Mid = 4.05, EffectiveDate = dateFrom },
                new CurrencyRate { Mid = 4.10, EffectiveDate = dateTo }
            }
        };

        _queryExecutorMock.Setup(x => x.Execute(
                It.IsAny<GetSeriesCurrencyRateFromToQuery>(),
                _nbpApiRestServiceMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(expectedData);

        _queryExecutorMock.Verify(x => x.Execute(
            It.IsAny<GetSeriesCurrencyRateFromToQuery>(),
            _nbpApiRestServiceMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenQueryExecutorFails()
    {
        // Arrange
        var request = new GetSeriesCurrencyRateFromToRequest("A", "EUR", DateTime.Now, DateTime.Now);
        var handler = new GetSeriesCurrencyRateFromToHandler(_queryExecutorMock.Object, _options, _nbpApiRestServiceMock.Object);

        _queryExecutorMock.Setup(x => x.Execute(
                It.IsAny<GetSeriesCurrencyRateFromToQuery>(),
                _nbpApiRestServiceMock.Object,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid date range"));

        // Act
        Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Invalid date range");
    }
}