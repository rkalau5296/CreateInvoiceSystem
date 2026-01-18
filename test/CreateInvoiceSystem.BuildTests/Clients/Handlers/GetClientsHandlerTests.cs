using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.BuildTests.Base;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Clients.Handlers;

public class GetClientsHandlerTests : BaseTest<IClientRepository>
{
    private readonly Mock<IQueryExecutor> _queryExecutorMock;
    private readonly GetClientsHandler _sut;

    public GetClientsHandlerTests()
    {
        // Arrange
        _queryExecutorMock = new Mock<IQueryExecutor>();
        _sut = new GetClientsHandler(_queryExecutorMock.Object, RepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnClientDtoList_WhenClientsExist()
    {
        // Arrange
        var userId = 100;
        var request = new GetClientsRequest { UserId = userId };

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

        _queryExecutorMock.Setup(q => q.Execute(
                It.IsAny<GetClientsQuery>(),
                RepositoryMock.Object,
                CancellationToken))
            .ReturnsAsync(clients);

        // Act
        var result = await _sut.Handle(request, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.Should().Contain(c => c.Name == "Client 1");
        result.Data.Should().Contain(c => c.Name == "Client 2");

        _queryExecutorMock.Verify(q => q.Execute(
            It.Is<GetClientsQuery>(query => query.UserId == userId),
            RepositoryMock.Object,
            CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoClientsFound()
    {
        // Arrange
        var request = new GetClientsRequest { UserId = 1 };
        _queryExecutorMock.Setup(q => q.Execute(It.IsAny<GetClientsQuery>(), RepositoryMock.Object, CancellationToken))
            .ReturnsAsync(new List<Client>());

        // Act
        var result = await _sut.Handle(request, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToQueryExecutor()
    {
        // Arrange
        var request = new GetClientsRequest { UserId = 1 };
        using var cts = new CancellationTokenSource();

        _queryExecutorMock.Setup(q => q.Execute(It.IsAny<GetClientsQuery>(), RepositoryMock.Object, cts.Token))
            .ReturnsAsync(new List<Client>());

        // Act
        await _sut.Handle(request, cts.Token);

        // Assert
        _queryExecutorMock.Verify(q => q.Execute(
            It.IsAny<GetClientsQuery>(),
            RepositoryMock.Object,
            cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenQueryExecutorThrows()
    {
        // Arrange
        var request = new GetClientsRequest { UserId = 1 };
        var errorMessage = "Database error";

        _queryExecutorMock.Setup(q => q.Execute(
                It.IsAny<GetClientsQuery>(),
                RepositoryMock.Object,
                CancellationToken))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var act = async () => await _sut.Handle(request, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage(errorMessage);
    }
}