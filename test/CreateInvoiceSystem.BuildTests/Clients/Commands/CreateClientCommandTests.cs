using CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Clients.Commands;

public class CreateClientCommandTests
{
    private readonly Mock<IClientRepository> _repositoryMock;

    public CreateClientCommandTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
    }
    
    [Fact]
    public async Task Execute_ShouldCreateAndReturnClient_WhenDataIsValid()
    {
        // Arrange        
        var addressDto = new AddressDto(1, "Testowa", "1", "Miasto", "00-000", "Polska");
        var createDto = new CreateClientDto("Nowy Klient", "1234567890", addressDto, 1, false);

        var command = new CreateClientCommand { Parametr = createDto };
        
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

        _repositoryMock.Setup(r => r.GetByIdAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntity);

        // Act
        var result = await command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(1);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenClientAlreadyExists()
    {
        // Arrange
        var addressDto = new AddressDto(0, "Testowa", "1", "Miasto", "00-000", "Polska");
        var createDto = new CreateClientDto("Istniejący", "123", addressDto, 1, false);
        var command = new CreateClientCommand { Parametr = createDto };

        _repositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A client with the same name and address already exists.");
    }

    [Fact]
    public async Task Execute_ShouldThrowArgumentNullException_WhenParametrIsNull()
    {
        // Arrange
        var command = new CreateClientCommand { Parametr = null! };

        // Act
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert        
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("Parametr");
    }

    [Fact]
    public async Task Execute_ShouldThrowArgumentNullException_WhenAddressIsNull()
    {
        // Arrange
        var createDto = new CreateClientDto("Test", "123", null!, 1, false);
        var command = new CreateClientCommand { Parametr = createDto };

        // Act
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert        
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("Address");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenClientCouldNotBeReloaded()
    {
        // Arrange
        var addressDto = new AddressDto(0, "Test", "1", "Test", "00-000", "PL");
        var createDto = new CreateClientDto("Test", "123", addressDto, 1, false);
        var command = new CreateClientCommand { Parametr = createDto };

        _repositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client { ClientId = 99, UserId = 1 });

        _repositoryMock.Setup(r => r.GetByIdAsync(99, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client)null!);

        // Act
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Client was saved but could not be reloaded.");
    }
}