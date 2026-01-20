using CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.UpdateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.CQRS;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Products.Handlers;

public class UpdateProductHandlerTests
{
    private readonly Mock<ICommandExecutor> _executorMock;
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly UpdateProductHandler _handler;

    public UpdateProductHandlerTests()
    {
        _executorMock = new Mock<ICommandExecutor>();
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new UpdateProductHandler(_executorMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldExecuteUpdateCommand_AndReturnUpdatedDto()
    {
        // Arrange
        var productId = 10;
        var inputDto = new UpdateProductDto(0, "Nowa Nazwa", "Opis", 150m, 1, false);
        var request = new UpdateProductRequest(productId, inputDto);
        
        var expectedDto = new UpdateProductDto(productId, "Nowa Nazwa", "Opis", 150m, 1, false);
        
        _executorMock
            .Setup(x => x.Execute<UpdateProductDto, UpdateProductDto, IProductRepository>(
                It.IsAny<CommandBase<UpdateProductDto, UpdateProductDto, IProductRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.ProductId.Should().Be(productId);
        result.Data.Name.Should().Be("Nowa Nazwa");

        _executorMock.Verify(x => x.Execute<UpdateProductDto, UpdateProductDto, IProductRepository>(
            It.Is<UpdateProductCommand>(c => c.Parametr.ProductId == productId),
            _repositoryMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDtoIsNull()
    {
        // Act
        Action act = () => new UpdateProductRequest(1, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*productDto*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenIdIsZero()
    {
        // Arrange
        var dto = new UpdateProductDto(0, "Test", "Test", 10m, 1, false);

        // Act
        Action act = () => new UpdateProductRequest(0, dto);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenExecutorFails()
    {
        // Arrange
        var dto = new UpdateProductDto(1, "Test", "Test", 10m, 1, false);
        var request = new UpdateProductRequest(1, dto);

        _executorMock
            .Setup(x => x.Execute<UpdateProductDto, UpdateProductDto, IProductRepository>(
                It.IsAny<CommandBase<UpdateProductDto, UpdateProductDto, IProductRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}