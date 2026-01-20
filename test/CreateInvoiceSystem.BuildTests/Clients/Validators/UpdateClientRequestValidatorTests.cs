using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.UpdateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using FluentValidation.TestHelper;
using FluentAssertions;

namespace CreateInvoiceSystem.BuildTests.Clients.Validators;

public class UpdateClientRequestValidatorTests
{
    private readonly UpdateClientRequestValidator _validator;

    public UpdateClientRequestValidatorTests()
    {
        _validator = new UpdateClientRequestValidator();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDtoIsNull()
    {
        // Arrange & Act
        Action act = () => new UpdateClientRequest(null!, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithMessage("*Argument 'clientDto' for client update request (Id=1) cannot be null*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenIdIsLessThanOne()
    {
        // Arrange
        var clientDto = new UpdateClientDto(0, "Name", "1234567890", null!, 0, 1);

        // Act
        Action act = () => new UpdateClientRequest(clientDto, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("id");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var clientDto = new UpdateClientDto(1, "", "1234567890", null!, 0, 1);
        var request = new UpdateClientRequest(clientDto, 1);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Client.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Nip_Is_Invalid()
    {
        // Arrange
        var clientDto = new UpdateClientDto(1, "Test", "123", null!, 0, 1);
        var request = new UpdateClientRequest(clientDto, 1);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Client.Nip)
            .WithErrorMessage("The Nip number must contain exactly 10 digits.");
    }

    [Theory]
    [InlineData("00000")]
    [InlineData("12-3456")]
    public void Should_Have_Error_When_PostalCode_Is_Invalid(string invalidPostal)
    {
        // Arrange
        var addressDto = new AddressDto(1, "Street", "1", "City", invalidPostal, "Country");
        var clientDto = new UpdateClientDto(1, "Name", "1234567890", addressDto, 1, 1);
        var request = new UpdateClientRequest(clientDto, 1);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Client.Address.PostalCode)
            .WithErrorMessage("Postal code must be in the format XX-XXX.");
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Update_Request_Is_Valid()
    {
        // Arrange
        var addressDto = new AddressDto(1, "Ulica", "10", "Miasto", "12-345", "Kraj");
        var clientDto = new UpdateClientDto(1, "Zaktualizowany", "1234567890", addressDto, 1, 1);
        var request = new UpdateClientRequest(clientDto, 1);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}