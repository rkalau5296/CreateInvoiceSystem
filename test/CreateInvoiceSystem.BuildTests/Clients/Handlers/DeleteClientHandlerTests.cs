using CreateInvoiceSystem.BuildTests.Base;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.DeleteClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Clients.Handlers;

public class DeleteClientHandlerTests : BaseTest<IClientRepository>
{
    private readonly DeleteClientHandler _sut;

    public DeleteClientHandlerTests()
    {
        // Arrange
        _sut = new DeleteClientHandler(ExecutorMock.Object, RepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnClientDto_WhenClientIsDeletedSuccessfully()
    {
        // Arrange
        var clientId = 10;
        var userId = 100;
        var request = new DeleteClientRequest(clientId) { UserId = userId };

        var address = new AddressDto(1, "Testowa", "1", "Miasto", "00-000", "Polska");
        var expectedResult = new ClientDto(clientId, "Firma do usunięcia", "1234567890", address, userId, true);

        ExecutorMock.Setup(e => e.Execute(
                It.IsAny<DeleteClientCommand>(),
                RepositoryMock.Object,
                CancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Handle(request, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(expectedResult);

        ExecutorMock.Verify(e => e.Execute(
            It.Is<DeleteClientCommand>(c => c.Parametr.ClientId == clientId && c.Parametr.UserId == userId),
            RepositoryMock.Object,
            CancellationToken), Times.Once);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenIdIsLessThanOne()
    {
        // Arrange
        int invalidId = 0;

        // Act
        Action act = () => new DeleteClientRequest(invalidId);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("id");
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToExecutor()
    {
        // Arrange
        var request = new DeleteClientRequest(1) { UserId = 1 };
        using var cts = new CancellationTokenSource();

        // Act
        await _sut.Handle(request, cts.Token);

        // Assert
        ExecutorMock.Verify(e => e.Execute(
            It.IsAny<DeleteClientCommand>(),
            RepositoryMock.Object,
            cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenDeletionFails()
    {
        // Arrange
        var request = new DeleteClientRequest(1) { UserId = 1 };
        var errorMessage = "Client not found";

        ExecutorMock.Setup(e => e.Execute(
                It.IsAny<DeleteClientCommand>(),
                RepositoryMock.Object,
                CancellationToken))
            .ThrowsAsync(new KeyNotFoundException(errorMessage));

        // Act
        var act = async () => await _sut.Handle(request, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage(errorMessage);
    }
}