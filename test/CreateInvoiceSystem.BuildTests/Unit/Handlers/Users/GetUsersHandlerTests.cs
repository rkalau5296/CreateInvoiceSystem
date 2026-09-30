using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

// Alias zapobiegający konfliktom nazw domenowych z systemowymi
using User = CreateInvoiceSystem.Modules.Users.Domain.Entities.User;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Users;

public class GetUsersHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly GetUsersHandler _sut;

    public GetUsersHandlerTests()
    {
        _sut = new GetUsersHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnGetUsersResponseWithMappedUserList_WhenUsersExist()
    {
        // Arrange
        var request = new GetUsersRequest();

        var usersInDb = new List<User>
        {
            new()
            {
                UserId = 1,
                Name = "Jan Kowalski",
                CompanyName = "Firma A",
                Email = "jan@example.com",
                Nip = "1234567890",
                BankAccountNumber = "PL12345678901234567890123456",
                Address = CreateTestAddress(1)
            },
            new()
            {
                UserId = 2,
                Name = "Anna Nowak",
                CompanyName = "Firma B",
                Email = "anna@example.com",
                Nip = "0987654321",
                BankAccountNumber = "PL65432109876543210987654321",
                Address = CreateTestAddress(2)
            }
        };

        _userRepositoryMock
            .Setup(r => r.GetUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(usersInDb);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);

        result.Data[0].UserId.Should().Be(1);
        result.Data[0].Name.Should().Be("Jan Kowalski");
        result.Data[0].Address.Should().NotBeNull();

        result.Data[1].UserId.Should().Be(2);
        result.Data[1].Name.Should().Be("Anna Nowak");
        result.Data[1].Address.Should().NotBeNull();

        _userRepositoryMock.Verify(
            r => r.GetUsersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListInResponse_WhenRepositoryReturnsEmptyList()
    {
        // Arrange
        var request = new GetUsersRequest();
        var emptyUserList = new List<User>();

        _userRepositoryMock
            .Setup(r => r.GetUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyUserList);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();

        _userRepositoryMock.Verify(
            r => r.GetUsersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenRepositoryReturnsNull()
    {
        // Arrange
        var request = new GetUsersRequest();

        _userRepositoryMock
            .Setup(r => r.GetUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<User>?)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("List of users is empty.");

        _userRepositoryMock.Verify(
            r => r.GetUsersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var request = new GetUsersRequest();
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        var usersInDb = new List<User>
        {
            new()
            {
                UserId = 1,
                Name = "Piotr Wiśniewski",
                Address = CreateTestAddress(1)
            }
        };

        _userRepositoryMock
            .Setup(r => r.GetUsersAsync(cancellationToken))
            .ReturnsAsync(usersInDb);

        // Act
        await _sut.Handle(request, cancellationToken);

        // Assert
        _userRepositoryMock.Verify(
            r => r.GetUsersAsync(cancellationToken),
            Times.Once);
    }

    #region Helper Methods

    private static Address CreateTestAddress(int id = 1) => new()
    {
        AddressId = id,
        Street = "Testowa",
        Number = "10",
        City = "Warszawa",
        PostalCode = "00-001",
        Country = "Polska"
    };

    #endregion
}