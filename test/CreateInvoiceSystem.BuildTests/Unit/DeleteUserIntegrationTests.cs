using CreateInvoiceSystem.Abstractions.Notification;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.DeleteUser;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;
using User = CreateInvoiceSystem.Modules.Users.Domain.Entities.User;
using UserInvoice = CreateInvoiceSystem.Modules.Users.Domain.Entities.Invoice;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class DeleteUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly DeleteUserHandler _sut;

    public DeleteUserHandlerTests()
    {
        _sut = new DeleteUserHandler(_userRepositoryMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldPublishNotificationAndDeleteUser_WhenUserHasNoAssociatedData()
    {
        // Arrange
        const int userId = 123;
        const int addressId = 456;
        var request = new DeleteUserRequest(userId);

        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            AddressId = addressId,
            Address = new Address { AddressId = addressId },
            Invoices = [],
            Clients = [],
            Products = []
        };

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => user);

        _userRepositoryMock
            .Setup(r => r.RemoveAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(r => r.RemoveAddress(addressId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().Be(userId);

        _mediatorMock.Verify(
            m => m.Publish(It.Is<UserDeletedNotification>(n => n.UserId == userId), It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(r => r.RemoveAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.RemoveAddress(addressId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenUserDoesNotExist()
    {
        // Arrange
        const int userId = 123;
        var request = new DeleteUserRequest(userId);

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"User with ID {userId} not found.");

        _userRepositoryMock.Verify(r => r.RemoveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenUserHasAssociatedData()
    {
        // Arrange
        const int userId = 123;
        var request = new DeleteUserRequest(userId);

        var user = new User
        {
            UserId = userId,
            Invoices = [new UserInvoice()],
            Clients = [],
            Products = []
        };

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => user);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Cannot delete User with ID {userId} because it has associated data.");

        _userRepositoryMock.Verify(r => r.RemoveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class UserDeletedClientsHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepoMock = new();

    [Fact]
    public async Task Handle_ShouldRemoveAllClientsByUserId()
    {
        // Arrange
        const int userId = 123;
        var notification = new UserDeletedNotification(userId);
        var handler = new UserDeletedClientsHandler(_clientRepoMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        _clientRepoMock.Verify(x => x.RemoveAllByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}