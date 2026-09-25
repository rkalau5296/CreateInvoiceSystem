using CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.CreateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class CreateClientHandlerTests
{
    private readonly Mock<IClientRepository> _repositoryMock;
    private readonly CreateClientHandler _sut;

    public CreateClientHandlerTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
        _sut = new CreateClientHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateAndReturnClient_WhenDataIsValid()
    {
        // Arrange
        var addressDto = new AddressDto(1, "Testowa", "1", "Miasto", "00-000", "Polska");
        var createDto = new CreateClientDto("Nowy Klient", "1234567890", addressDto, 1, "testc@test.com");
        var request = new CreateClientRequest(createDto) { UserId = 1 };

        var savedEntity = new Client
        {
            ClientId = 1,
            Name = "Nowy Klient",
            UserId = 1,
            Address = new Address
            {
                AddressId = 1,
                Street = "Testowa",
                Number = "1",
                City = "Miasto",
                PostalCode = "00-000",
                Country = "Polska"
            }
        };

        _repositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntity);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.ClientId.Should().Be(1);
        result.Data.Name.Should().Be("Nowy Klient");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenClientAlreadyExists()
    {
        // Arrange
        var addressDto = new AddressDto(0, "Testowa", "1", "Miasto", "00-000", "Polska");
        var createDto = new CreateClientDto("Istniejący", "123", addressDto, 1, "testc@test.com");
        var request = new CreateClientRequest(createDto) { UserId = 1 };

        _repositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Istnieje już taki klient z identycznymi danymi.");
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenClientIsNull()
    {
        // Arrange
        var request = new CreateClientRequest(null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request.Client");
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenAddressIsNull()
    {
        // Arrange
        var createDto = new CreateClientDto("Test", "123", null!, 1, "testc@test.com");
        var request = new CreateClientRequest(createDto);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("Address");
    }
}