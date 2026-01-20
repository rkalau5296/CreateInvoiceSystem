using CreateInvoiceSystem.Modules.Products.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.CQRS;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Products.Handlers;

public class GetProductHandlerTests
{
    private readonly Mock<IQueryExecutor> _queryExecutorMock;
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly GetProductHandler _handler;

    public GetProductHandlerTests()
    {
        _queryExecutorMock = new Mock<IQueryExecutor>();
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductHandler(_queryExecutorMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnProductDto_WhenProductExists()
    {
        // Arrange
        var request = new GetProductRequest(1) { UserId = 100 };
        var productEntity = new Product
        {
            ProductId = 1,
            Name = "Laptop",
            Description = "Opis",
            Value = 3500m,
            UserId = 100
        };

        _queryExecutorMock
            .Setup(x => x.Execute<Product, IProductRepository>(
                It.IsAny<QueryBase<Product, IProductRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(productEntity);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.ProductId.Should().Be(1);
        result.Data.Name.Should().Be("Laptop");
        result.Data.Value.Should().Be(3500m);
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectParameters_ToQuery()
    {
        // Arrange
        var request = new GetProductRequest(5) { UserId = 500 };

        _queryExecutorMock
            .Setup(x => x.Execute<Product, IProductRepository>(
                It.IsAny<QueryBase<Product, IProductRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { ProductId = 5 });

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert         
        _queryExecutorMock.Verify(x => x.Execute<Product, IProductRepository>(
            It.IsAny<GetProductQuery>(),
            _repositoryMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenProductNotFound()
    {
        // Arrange
        var request = new GetProductRequest(99);

        _queryExecutorMock
            .Setup(x => x.Execute<Product, IProductRepository>(
                It.IsAny<QueryBase<Product, IProductRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!); // Executor zwraca null

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("product");
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        // Arrange
        var request = new GetProductRequest(1);
        using var cts = new CancellationTokenSource();
        cts.Cancel(); 

        _queryExecutorMock
            .Setup(x => x.Execute<Product, IProductRepository>(
                It.IsAny<QueryBase<Product, IProductRepository>>(),
                _repositoryMock.Object,
                cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        Func<Task> act = async () => await _handler.Handle(request, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}