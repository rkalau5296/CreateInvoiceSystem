namespace CreateInvoiceSystem.BuildTests.Users.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.Notification;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.DeleteUser;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;
using Moq;
using Xunit;

public class DeleteUserIntegrationTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ICommandExecutor> _executorMock = new();
    private readonly Mock<IClientRepository> _clientRepoMock = new();

    [Fact]
    public async Task Handle_ShouldPublishNotificationAndReturnDataFromExecutor()
    {
        // Arrange
        int userId = 123;
        var request = new DeleteUserRequest(userId);
        var cancellationToken = CancellationToken.None;

        var expectedDto = new UserDto(
            userId, "Test", "Test Corp", "test@test.pl", "pass", "123",
            null, "123", true, [], [], []
        );

        _executorMock.Setup(x => x.Execute(
                It.IsAny<DeleteUserCommand>(),
                _userRepoMock.Object,
                cancellationToken))
            .ReturnsAsync(expectedDto);

        var handler = new DeleteUserHandler(_executorMock.Object, _userRepoMock.Object, _mediatorMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert        
        _mediatorMock.Verify(x => x.Publish(
            It.Is<INotification>(n => n.GetType().Name == "UserDeletedNotification"),
            cancellationToken),
            Times.Once);

        _executorMock.Verify(x => x.Execute(
            It.Is<DeleteUserCommand>(c => c.Parametr.UserId == userId),
            _userRepoMock.Object,
            cancellationToken),
            Times.Once);

        Assert.NotNull(result.Data);
        Assert.Equal(userId, result.Data.UserId);
    }

    [Fact]
    public async Task Handle_ShouldOnlyRemoveClients_BecauseTransactionBehaviorSavesChanges()
    {
        // Arrange
        int userId = 123;
        var notification = new UserDeletedNotification(userId);
        var handler = new UserDeletedClientsHandler(_clientRepoMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert        
        _clientRepoMock.Verify(x => x.RemoveAllByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}