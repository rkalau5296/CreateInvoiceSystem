using CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class GetProductHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly GetProductHandler _sut;

    public GetProductHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _sut = new GetProductHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnProductDto_WhenProductExists()
    {
        // Arrange
        const int productId = 1;
        const int userId = 100;
        var request = new GetProductRequest(productId) { UserId = userId };

        var expectedProduct = new Product
        {
            ProductId = productId,
            Name = "Laptop",
            Description = "Opis",
            Value = 3500m,
            UserId = userId
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data!.ProductId.Should().Be(productId);
        result.Data.Name.Should().Be("Laptop");
        result.Data.Description.Should().Be("Opis");
        result.Data.Value.Should().Be(3500m);
        result.Data.UserId.Should().Be(userId);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(productId, It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenProductNotFound()
    {
        // Arrange
        const int productId = 99;
        const int userId = 1;
        var request = new GetProductRequest(productId) { UserId = userId };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Product with ID {productId} not found.");
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var request = new GetProductRequest(1) { UserId = 1 };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int?>(), cts.Token))
            .ReturnsAsync(new Product { ProductId = 1, Name = "Test" });

        // Act
        await _sut.Handle(request, cts.Token);

        // Assert
        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int?>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken_WhenOperationIsCanceled()
    {
        // Arrange
        var request = new GetProductRequest(1) { UserId = 1 };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int?>(), cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        Func<Task> act = async () => await _sut.Handle(request, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}