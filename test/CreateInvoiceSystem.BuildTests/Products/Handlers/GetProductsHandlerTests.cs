using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Products.Handlers;

public class GetProductsHandlerTests
{
    private readonly Mock<IQueryExecutor> _queryExecutorMock;
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly GetProductsHandler _handler;

    public GetProductsHandlerTests()
    {
        _queryExecutorMock = new Mock<IQueryExecutor>();
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductsHandler(_queryExecutorMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnProductDtoList_WhenProductsExist()
    {
        // Arrange
        var request = new GetProductsRequest { UserId = 1, PageNumber = 1, PageSize = 10 };
        var productsFromDb = new List<Product>
    {
        new() { ProductId = 1, Name = "Produkt 1", UserId = 1 },
        new() { ProductId = 2, Name = "Produkt 2", UserId = 1 }
    };
        
        var pagedResult = new PagedResult<Product>(productsFromDb, 2, 1, 10);

        _queryExecutorMock
            .Setup(x => x.Execute<PagedResult<Product>, IProductRepository>(
                It.IsAny<GetProductsQuery>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.First().Name.Should().Be("Produkt 1");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoProductsFound()
    {
        // Arrange
        var request = new GetProductsRequest { UserId = 999, PageNumber = 1, PageSize = 10 };
        
        var emptyPagedResult = new PagedResult<Product>(new List<Product>(), 0, 1, 10);

        _queryExecutorMock
            .Setup(x => x.Execute<PagedResult<Product>, IProductRepository>(
                It.IsAny<GetProductsQuery>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallExecute_WithAnyGetProductsQuery()
    {
        // Arrange
        var request = new GetProductsRequest { UserId = 1, PageNumber = 1, PageSize = 10 };
        var emptyPagedResult = new PagedResult<Product>(new List<Product>(), 0, 1, 10);

        _queryExecutorMock
            .Setup(x => x.Execute<PagedResult<Product>, IProductRepository>(
                It.IsAny<GetProductsQuery>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedResult);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _queryExecutorMock.Verify(x => x.Execute<PagedResult<Product>, IProductRepository>(
            It.IsAny<GetProductsQuery>(),
            _repositoryMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}