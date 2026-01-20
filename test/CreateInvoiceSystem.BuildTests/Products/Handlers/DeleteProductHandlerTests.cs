using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.DeleteProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Products.Handlers;

public class DeleteProductHandlerTests
{
    private readonly Mock<ICommandExecutor> _executorMock;
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly DeleteProductHandler _handler;

    public DeleteProductHandlerTests()
    {
        _executorMock = new Mock<ICommandExecutor>();
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new DeleteProductHandler(_executorMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldExecuteDeleteCommand_AndReturnProductDto()
    {
        // Arrange
        var productId = 10;
        var userId = 1;
        var request = new DeleteProductRequest(productId) { UserId = userId };

        var expectedDto = new ProductDto(productId, "Usunięty", "Opis", 0, userId, true);

        _executorMock
            .Setup(x => x.Execute<Product, ProductDto, IProductRepository>(
                It.IsAny<CommandBase<Product, ProductDto, IProductRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.ProductId.Should().Be(productId);
        result.Data.IsDeleted.Should().BeTrue();

        _executorMock.Verify(x => x.Execute<Product, ProductDto, IProductRepository>(
            It.Is<CommandBase<Product, ProductDto, IProductRepository>>(c =>
                c.Parametr.ProductId == productId && c.Parametr.UserId == userId),
            _repositoryMock.Object,
            It.IsAny<CancellationToken>()),
        Times.Once);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenIdIsLessThanOne()
    {
        // Act
        Action act = () => new DeleteProductRequest(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("id");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenExecutorFails()
    {
        // Arrange
        var request = new DeleteProductRequest(1) { UserId = 1 };

        _executorMock
            .Setup(x => x.Execute<Product, ProductDto, IProductRepository>(
                It.IsAny<CommandBase<Product, ProductDto, IProductRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}