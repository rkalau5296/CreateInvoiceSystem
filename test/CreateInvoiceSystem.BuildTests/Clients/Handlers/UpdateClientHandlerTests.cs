using CreateInvoiceSystem.BuildTests.Base;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.UpdateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Clients.Handlers;

public class UpdateClientHandlerTests : BaseTest<IClientRepository>
{
    private readonly UpdateClientHandler _sut;

    public UpdateClientHandlerTests()
    {
        _sut = new UpdateClientHandler(ExecutorMock.Object, RepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnData_WhenUpdateIsSuccessful()
    {
        // Arrange
        var clientId = 1;
        var address = new AddressDto(50, "Testowa", "1", "Miasto", "00-000", "Polska");
        var updateDto = new UpdateClientDto(0, "Firma", "123", address, 50, 100);
        var request = new UpdateClientRequest(updateDto, clientId);
        
        var expectedResult = updateDto with { ClientId = clientId };

        ExecutorMock.Setup(e => e.Execute(
                It.IsAny<UpdateClientCommand>(),
                RepositoryMock.Object,
                CancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Handle(request, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToExecutor()
    {
        // Arrange
        var address = new AddressDto(1, "", "", "", "", "");
        var updateDto = new UpdateClientDto(1, "", "", address, 1, 1);
        var request = new UpdateClientRequest(updateDto, 1);
        using var cts = new CancellationTokenSource();

        ExecutorMock.Setup(e => e.Execute(
                It.IsAny<UpdateClientCommand>(),
                RepositoryMock.Object,
                cts.Token))
            .ReturnsAsync(updateDto);

        // Act
        await _sut.Handle(request, cts.Token);

        // Assert
        ExecutorMock.Verify(e => e.Execute(
            It.IsAny<UpdateClientCommand>(),
            RepositoryMock.Object,
            cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenExecutorThrows()
    {
        // Arrange
        var address = new AddressDto(1, "", "", "", "", "");
        var updateDto = new UpdateClientDto(1, "", "", address, 1, 1);
        var request = new UpdateClientRequest(updateDto, 1);

        ExecutorMock.Setup(e => e.Execute(
                It.IsAny<UpdateClientCommand>(),
                RepositoryMock.Object,
                CancellationToken))
            .ThrowsAsync(new Exception("Błąd bazy danych"));

        // Act
        var act = async () => await _sut.Handle(request, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenIdIsLessThanOne()
    {
        // Arrange
        var address = new AddressDto(1, "", "", "", "", "");
        var updateDto = new UpdateClientDto(0, "", "", address, 1, 1);

        // Act
        Action act = () => new UpdateClientRequest(updateDto, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Id must be greater than or equal to 1.*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenClientDtoIsNull()
    {
        // Act
        Action act = () => new UpdateClientRequest(null!, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*cannot be null*");
    }
}