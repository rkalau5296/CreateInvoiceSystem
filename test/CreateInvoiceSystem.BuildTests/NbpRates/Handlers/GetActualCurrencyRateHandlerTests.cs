using Moq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRate;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;

namespace CreateInvoiceSystem.BuildTests.NbpRates.Handlers;

public class GetActualCurrencyRateHandlerTests
{
    private readonly Mock<IQueryExecutor> _queryExecutorMock;
    private readonly Mock<INbpApiRestService> _nbpApiRestServiceMock;
    private readonly IOptions<NbpApiOptions> _options;

    public GetActualCurrencyRateHandlerTests()
    {
        _queryExecutorMock = new Mock<IQueryExecutor>();
        _nbpApiRestServiceMock = new Mock<INbpApiRestService>();

        var nbpOptions = new NbpApiOptions { BaseUrl = "http://api.nbp.pl/" };
        _options = Options.Create(nbpOptions);
    }

    [Fact]
    public async Task Handle_ShouldReturnCurrencyRateResponse_WhenQueryIsSuccessful()
    {
        // Arrange
        var request = new GetActualCurrencyRateRequest("A", "USD");
        var handler = new GetActualCurrencyRateHandler(_queryExecutorMock.Object, _options, _nbpApiRestServiceMock.Object);

        var expectedData = new CurrencyRatesTable
        {
            Table = "A",
            Code = "USD",
            CurrencyCode = "USD",
            Rates = new List<CurrencyRate>
            {
                new CurrencyRate { Mid = 4.05, EffectiveDate = DateTime.Now }
            }
        };

        _queryExecutorMock.Setup(x => x.Execute(
                It.IsAny<GetActualCurrencyRateQuery>(),
                _nbpApiRestServiceMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(expectedData);

        _queryExecutorMock.Verify(x => x.Execute(
            It.Is<GetActualCurrencyRateQuery>(q => true),
            _nbpApiRestServiceMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenQueryExecutorFails()
    {
        // Arrange
        var request = new GetActualCurrencyRateRequest("A", "EUR");
        var handler = new GetActualCurrencyRateHandler(_queryExecutorMock.Object, _options, _nbpApiRestServiceMock.Object);

        _queryExecutorMock.Setup(x => x.Execute(
                It.IsAny<GetActualCurrencyRateQuery>(),
                _nbpApiRestServiceMock.Object,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API Error"));

        // Act
        Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("API Error");
    }
}