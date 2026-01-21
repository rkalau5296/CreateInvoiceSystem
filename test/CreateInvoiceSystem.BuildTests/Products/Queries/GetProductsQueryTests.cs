using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Products.Queries;

public class GetProductsQueryTests
{
    private readonly Mock<IProductRepository> _repositoryMock;

    public GetProductsQueryTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
    }

    [Fact]
    public async Task Execute_ShouldReturnPagedResultOfProducts_WhenProductsExist()
    {
        // Arrange
        var userId = 123;
        var pageNumber = 1;
        var pageSize = 10;
        var searchTerm = "test"; 
        var query = new GetProductsQuery(userId, pageNumber, pageSize, searchTerm);

        var products = new List<Product>
        {
            new() { ProductId = 1, Name = "Produkt A", UserId = userId },
            new() { ProductId = 2, Name = "Produkt B", UserId = userId }
        };
        var pagedResult = new PagedResult<Product>(products, 2, pageNumber, pageSize);

        _repositoryMock
            .Setup(r => r.GetAllAsync(userId, pageNumber, pageSize, searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);

        _repositoryMock.Verify(r => r.GetAllAsync(userId, pageNumber, pageSize, searchTerm, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenRepositoryReturnsNull()
    {
        // Arrange
        var userId = 1;        
        var query = new GetProductsQuery(userId, 1, 10, null);

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Product>)null!);

        // Act
        Func<Task> act = async () => await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("List of products is empty.");
    }

    [Fact]
    public async Task Execute_ShouldHandleNullUserId()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;        
        var query = new GetProductsQuery(null, pageNumber, pageSize, null);
        var pagedResult = new PagedResult<Product>(new List<Product> { new() { ProductId = 1 } }, 1, pageNumber, pageSize);

        _repositoryMock
            .Setup(r => r.GetAllAsync(null, pageNumber, pageSize, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetAllAsync(null, pageNumber, pageSize, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}