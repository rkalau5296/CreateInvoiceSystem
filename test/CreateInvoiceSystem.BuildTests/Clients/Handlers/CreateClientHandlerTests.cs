using CreateInvoiceSystem.BuildTests.Base;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.CreateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using FluentAssertions;
using Moq;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;

namespace CreateInvoiceSystem.BuildTests.Clients.Handlers;

public class CreateClientHandlerTests : BaseTest<IClientRepository>
{
    private readonly CreateClientHandler _sut;

    public CreateClientHandlerTests()
    {        
        _sut = new CreateClientHandler(ExecutorMock.Object, RepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnClientDto_WhenCommandExecutesSuccessfully()
    {
        // Arrange
        var address = new AddressDto(1, "Warszawska", "10A", "Warszawa", "00-001", "Polska");        
        var createDto = new CreateClientDto("Firma XYZ", "1234567890", address, 1, false);        
        var request = new CreateClientRequest(createDto) { UserId = 1 };        
        var expectedResult = new ClientDto(1, "Firma XYZ", "1234567890", address, 1, false);

        ExecutorMock.Setup(e => e.Execute(
                It.IsAny<CreateClientCommand>(),
                RepositoryMock.Object,
                CancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Handle(request, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(expectedResult);        
        result.Data.ClientId.Should().Be(1);
        result.Data.Name.Should().Be("Firma XYZ");
        
        ExecutorMock.Verify(e => e.Execute(
            It.Is<CreateClientCommand>(c => c.Parametr == createDto),
            RepositoryMock.Object,
            CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToExecutor()
    {
        // Arrange
        var address = new AddressDto(0, "", "", "", "", "");
        var createDto = new CreateClientDto("", "", address, 1, false);
        var request = new CreateClientRequest(createDto);
        using var cts = new CancellationTokenSource();

        // Act
        await _sut.Handle(request, cts.Token);

        // Assert
        ExecutorMock.Verify(e => e.Execute(
            It.IsAny<CreateClientCommand>(),
            RepositoryMock.Object,
            cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenExecutorThrows()
    {
        // Arrange
        var address = new AddressDto(0, "", "", "", "", "");
        var createDto = new CreateClientDto("Error", "000", address, 1, false);
        var request = new CreateClientRequest(createDto);

        ExecutorMock.Setup(e => e.Execute(
                It.IsAny<CreateClientCommand>(),
                RepositoryMock.Object,
                CancellationToken))
            .ThrowsAsync(new InvalidOperationException("Business Logic Error"));

        // Act
        var act = async () => await _sut.Handle(request, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Business Logic Error");
    }
}