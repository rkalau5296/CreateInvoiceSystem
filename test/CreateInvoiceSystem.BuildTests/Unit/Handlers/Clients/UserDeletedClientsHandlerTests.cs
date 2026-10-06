using CreateInvoiceSystem.Abstractions.Notification;
using CreateInvoiceSystem.Modules.Clients.Application.Handlers;
using CreateInvoiceSystem.Modules.Clients.Interfaces;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Clients;

public class UserDeletedClientsHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly UserDeletedClientsHandler _handler;

    public UserDeletedClientsHandlerTests()
    {
        _clientRepositoryMock = new Mock<IClientRepository>();
        _handler = new UserDeletedClientsHandler(_clientRepositoryMock.Object);
    }

    [Fact]
    public async Task Should_Call_RemoveAllByUserIdAsync_With_Correct_UserId()
    {
        var notification = new UserDeletedNotification(123);
        var cancellationToken = new CancellationToken();

        await _handler.Handle(notification, cancellationToken);

        _clientRepositoryMock.Verify(
            x => x.RemoveAllByUserIdAsync(123, cancellationToken),
            Times.Once);
    }
}