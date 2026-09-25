using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class GetProductsHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly GetProductsHandler _sut;

    public GetProductsHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _sut = new GetProductsHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnProductDtoPagedResult_WhenProductsExist()
    {
        // Arrange
        const int userId = 1;
        const int pageNumber = 1;
        const int pageSize = 10;
        const string searchTerm = "test";

        var request = new GetProductsRequest
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var productsFromDb = new List<Product>
        {
            new() { ProductId = 1, Name = "Produkt 1", UserId = userId },
            new() { ProductId = 2, Name = "Produkt 2", UserId = userId }
        };

        var pagedResult = new PagedResult<Product>(productsFromDb, 2, pageNumber, pageSize);

        _repositoryMock
            .Setup(r => r.GetAllAsync(userId, pageNumber, pageSize, searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.First().Name.Should().Be("Produkt 1");

        _repositoryMock.Verify(
            r => r.GetAllAsync(userId, pageNumber, pageSize, searchTerm, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoProductsFound()
    {
        // Arrange
        var request = new GetProductsRequest { UserId = 999, PageNumber = 1, PageSize = 10 };
        var emptyPagedResult = new PagedResult<Product>(new List<Product>(), 0, 1, 10);

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedResult);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldHandleNullUserId()
    {
        // Arrange
        const int pageNumber = 1;
        const int pageSize = 10;
        var request = new GetProductsRequest { UserId = null, PageNumber = pageNumber, PageSize = pageSize };

        var pagedResult = new PagedResult<Product>(new List<Product> { new() { ProductId = 1, Name = "Produkt" } }, 1, pageNumber, pageSize);

        _repositoryMock
            .Setup(r => r.GetAllAsync(null, pageNumber, pageSize, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(1);

        _repositoryMock.Verify(
            r => r.GetAllAsync(null, pageNumber, pageSize, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenRepositoryReturnsNull()
    {
        // Arrange
        var request = new GetProductsRequest { UserId = 1, PageNumber = 1, PageSize = 10 };

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Product>)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("List of products is empty.");
    }
}