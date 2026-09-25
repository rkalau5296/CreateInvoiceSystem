using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.BuildTests.Base;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class GetClientsHandlerTests : BaseTest<IClientRepository>
{
    private readonly GetClientsHandler _sut;

    public GetClientsHandlerTests()
    {
        _sut = new GetClientsHandler(RepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnClientDtoList_WhenClientsExist()
    {
        // Arrange
        var userId = 100;
        var pageNumber = 1;
        var pageSize = 10;
        var request = new GetClientsRequest { UserId = userId, PageNumber = pageNumber, PageSize = pageSize };

        var clients = new List<Client>
        {
            new Client
            {
                ClientId = 1,
                Name = "Client 1",
                Address = new Address { Street = "S1", City = "C1", Number = "1", PostalCode = "00-001", Country = "PL" }
            },
            new Client
            {
                ClientId = 2,
                Name = "Client 2",
                Address = new Address { Street = "S2", City = "C2", Number = "2", PostalCode = "00-002", Country = "PL" }
            }
        };
        var pagedResult = new PagedResult<Client>(clients, 2, pageNumber, pageSize);

        RepositoryMock.Setup(r => r.GetAllAsync(userId, pageNumber, pageSize, It.IsAny<string?>(), CancellationToken))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _sut.Handle(request, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.Should().Contain(c => c.Name == "Client 1");
        result.Data.Should().Contain(c => c.Name == "Client 2");

        RepositoryMock.Verify(r => r.GetAllAsync(userId, pageNumber, pageSize, It.IsAny<string?>(), CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoClientsFound()
    {
        // Arrange
        var userId = 1;
        var pageNumber = 1;
        var pageSize = 10;
        var request = new GetClientsRequest { UserId = userId, PageNumber = pageNumber, PageSize = pageSize };
        var emptyPagedResult = new PagedResult<Client>(new List<Client>(), 0, pageNumber, pageSize);

        RepositoryMock.Setup(r => r.GetAllAsync(userId, pageNumber, pageSize, It.IsAny<string?>(), CancellationToken))
            .ReturnsAsync(emptyPagedResult);

        // Act
        var result = await _sut.Handle(request, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenRepositoryReturnsNull()
    {
        // Arrange
        var userId = 1;
        var pageNumber = 1;
        var pageSize = 10;
        var request = new GetClientsRequest { UserId = userId, PageNumber = pageNumber, PageSize = pageSize };

        RepositoryMock.Setup(r => r.GetAllAsync(userId, pageNumber, pageSize, It.IsAny<string?>(), CancellationToken))
            .ReturnsAsync((PagedResult<Client>)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("List of clients is empty.");
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var request = new GetClientsRequest { UserId = 1, PageNumber = 1, PageSize = 10 };
        using var cts = new CancellationTokenSource();
        var emptyPagedResult = new PagedResult<Client>(new List<Client>(), 0, 1, 10);

        RepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), cts.Token))
            .ReturnsAsync(emptyPagedResult);

        // Act
        await _sut.Handle(request, cts.Token);

        // Assert
        RepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        var request = new GetClientsRequest { UserId = 1, PageNumber = 1, PageSize = 10 };
        var errorMessage = "Database error";

        RepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), CancellationToken))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage(errorMessage);
    }
}