using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.CreateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Products.Handlers;

public class CreateProductHandlerTests
{
    private readonly Mock<ICommandExecutor> _executorMock;
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _executorMock = new Mock<ICommandExecutor>();
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new CreateProductHandler(_executorMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldExecuteCommand_AndReturnResponse_WhenDataIsValid()
    {
        // Arrange
        var productDto = new CreateProductDto("Produkt A", "Opis", 100m, 1);
        var request = new CreateProductRequest(productDto);

        // Moq musi zwrócić CreateProductDto, bo tego oczekuje Twój Handler w linii: Data = ProductFromDb
        var expectedDto = new CreateProductDto("Produkt A", "Opis", 100m, 1);

        _executorMock
            .Setup(x => x.Execute<CreateProductDto, CreateProductDto, IProductRepository>(
                It.IsAny<CommandBase<CreateProductDto, CreateProductDto, IProductRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenExecutorFails()
    {
        // Arrange
        var productDto = new CreateProductDto("Błąd", "Opis", 10m, 1);
        var request = new CreateProductRequest(productDto);

        _executorMock
            .Setup(x => x.Execute<CreateProductDto, CreateProductDto, IProductRepository>(
                It.IsAny<CommandBase<CreateProductDto, CreateProductDto, IProductRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}