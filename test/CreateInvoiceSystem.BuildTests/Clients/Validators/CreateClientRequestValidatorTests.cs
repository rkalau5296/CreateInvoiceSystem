using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.CreateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using FluentValidation.TestHelper;

namespace CreateInvoiceSystem.BuildTests.Clients.Validators;

public class CreateClientRequestValidatorTests
{
    private readonly CreateClientRequestValidator _validator;

    public CreateClientRequestValidatorTests()
    {
        _validator = new CreateClientRequestValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var clientDto = new CreateClientDto("", "1234567890", null!, 1, false);
        var request = new CreateClientRequest(clientDto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Client.Name)
            .WithErrorMessage("Name is required.");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901")]
    [InlineData("ABCDEFGHIJ")]
    public void Should_Have_Error_When_Nip_Is_Invalid(string invalidNip)
    {
        // Arrange
        var clientDto = new CreateClientDto("Test", invalidNip, null!, 1, false);
        var request = new CreateClientRequest(clientDto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Client.Nip)
            .WithErrorMessage("The Nip number must contain exactly 10 digits.");
    }

    [Fact]
    public void Should_Have_Error_When_Address_Is_Null()
    {
        // Arrange
        var clientDto = new CreateClientDto("Test", "1234567890", null!, 1, false);
        var request = new CreateClientRequest(clientDto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Client.Address)
            .WithErrorMessage("Address must be specified.");
    }

    [Theory]
    [InlineData("00000")]
    [InlineData("00-0000")]
    [InlineData("AA-000")]
    public void Should_Have_Error_When_PostalCode_Is_Invalid(string invalidPostalCode)
    {
        // Arrange
        var addressDto = new AddressDto(0, "Street", "1", "City", invalidPostalCode, "Country");
        var clientDto = new CreateClientDto("Test", "1234567890", addressDto, 1, false);
        var request = new CreateClientRequest(clientDto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Client.Address.PostalCode)
            .WithErrorMessage("Postal code must be in the format XX-XXX.");
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        // Arrange
        var addressDto = new AddressDto(0, "Wiejska", "10", "Warszawa", "00-902", "Polska");
        var clientDto = new CreateClientDto("Poprawny Klient", "1234567890", addressDto, 1, false);
        var request = new CreateClientRequest(clientDto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}