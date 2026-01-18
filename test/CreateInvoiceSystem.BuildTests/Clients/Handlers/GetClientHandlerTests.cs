using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.BuildTests.Base;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Clients.Handlers;

public class GetClientHandlerTests : BaseTest<IClientRepository>
{
    private readonly Mock<IQueryExecutor> _queryExecutorMock;
    private readonly GetClientHandler _sut;

    public GetClientHandlerTests()
    {
        // Arrange
        _queryExecutorMock = new Mock<IQueryExecutor>();
        _sut = new GetClientHandler(_queryExecutorMock.Object, RepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnClientDto_WhenClientExists()
    {
        // Arrange
        var clientId = 1;
        var userId = 100;
        var request = new GetClientRequest(clientId) { UserId = userId };

        var clientEntity = new Client
        {
            ClientId = clientId,
            UserId = userId,
            Name = "Test Client",
            Address = new Address { Street = "Street", City = "City", Number = "1", PostalCode = "00-000", Country = "Poland" }
        };

        _queryExecutorMock.Setup(q => q.Execute(
                It.IsAny<GetClientQuery>(),
                RepositoryMock.Object,
                CancellationToken))
            .ReturnsAsync(clientEntity);

        // Act
        var result = await _sut.Handle(request, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.ClientId.Should().Be(clientId);
        result.Data.Name.Should().Be(clientEntity.Name);

        _queryExecutorMock.Verify(q => q.Execute(
            It.Is<GetClientQuery>(query => query.Id == clientId && query.UserId == userId),
            RepositoryMock.Object,
            CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToQueryExecutor()
    {
        // Arrange
        var request = new GetClientRequest(1) { UserId = 1 };
        using var cts = new CancellationTokenSource();

        var clientEntity = new Client
        {
            Address = new Address { Street = "Street", City = "City", Number = "1", PostalCode = "00-000", Country = "Poland" }
        };

        _queryExecutorMock.Setup(q => q.Execute(It.IsAny<GetClientQuery>(), RepositoryMock.Object, cts.Token))
            .ReturnsAsync(clientEntity);

        // Act
        await _sut.Handle(request, cts.Token);

        // Assert
        _queryExecutorMock.Verify(q => q.Execute(
            It.IsAny<GetClientQuery>(),
            RepositoryMock.Object,
            cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenQueryExecutorThrows()
    {
        // Arrange
        var request = new GetClientRequest(1) { UserId = 1 };
        var errorMessage = "Query execution failed";

        _queryExecutorMock.Setup(q => q.Execute(
                It.IsAny<GetClientQuery>(),
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