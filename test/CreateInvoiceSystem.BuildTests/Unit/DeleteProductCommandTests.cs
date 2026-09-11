using CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class DeleteProductCommandTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly DeleteProductCommand _command;

    public DeleteProductCommandTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _command = new DeleteProductCommand();
    }

    [Fact]
    public async Task Execute_ShouldReturnProductDto_WhenProductIsSuccessfullyDeleted()
    {
        // Arrange
        const int productId = 10;
        const int userId = 1;

        var product = new Product
        {
            ProductId = productId,
            Name = "Do usunięcia",
            UserId = userId
        };

        _command.Parametr = product;

        var existingProduct = new Product
        {
            ProductId = productId,
            Name = "Do usunięcia",
            UserId = userId
        };

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                productId,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _command.Execute(
            _repositoryMock.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProductId.Should().Be(productId);

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                productId,
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.RemoveAsync(
                productId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenProductDoesNotExist()
    {
        // Arrange
        var productParam = new Product { ProductId = 99, UserId = 1 };
        _command.Parametr = productParam;
        
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        // Act
        Func<Task> act = async () => await _command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Product with ID {productParam.ProductId} not found.");

        _repositoryMock.Verify(r => r.RemoveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldPropagateException_WhenDeleteFails()
    {
        // Arrange
        const int productId = 5;
        const int userId = 1;

        var product = new Product
        {
            ProductId = productId,
            UserId = userId
        };

        _command.Parametr = product;

        var existingProduct = new Product
        {
            ProductId = productId,
            UserId = userId
        };

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                productId,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock
            .Setup(repository => repository.RemoveAsync(
                productId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(
                $"Failed to delete Product with ID {productId}."));

        // Act
        Func<Task> act = () => _command.Execute(
            _repositoryMock.Object,
            CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Failed to delete Product with ID {productId}.");

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                productId,
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.RemoveAsync(
                productId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_ShouldThrowArgumentNullException_WhenParametrIsNull()
    {
        // Arrange
        _command.Parametr = null!;

        // Act
        Func<Task> act = async () => await _command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}