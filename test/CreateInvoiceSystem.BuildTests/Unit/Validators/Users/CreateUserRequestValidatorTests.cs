using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.CreateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Users;

public class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator = new();

    private static CreateUserDto CreateValidUserDto() => new(
        Name: "Jan Kowalski",
        CompanyName: "Acme Corp",
        Email: "jan.kowalski@example.com",
        Password: "SecurePassword123!",
        Nip: "1234567890",
        IsActive: true,
        Address: new CreateAddressDto(
            Street: "Marszałkowska",
            Number: "10/2",
            City: "Warszawa",
            PostalCode: "00-001",
            Country: "Polska"
        )
    );

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var user = CreateValidUserDto() with { Name = "" };
        var request = new CreateUserRequest(user);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Maximum_Length()
    {
        var user = CreateValidUserDto() with { Name = new string('a', 101) };
        var request = new CreateUserRequest(user);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Name);
    }

    [Fact]
    public void Should_Have_Error_When_CompanyName_Is_Empty()
    {
        var user = CreateValidUserDto() with { CompanyName = "" };
        var request = new CreateUserRequest(user);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.CompanyName)
            .WithErrorMessage("CompanyName is required.");
    }

    [Theory]
    [InlineData(null, "Email is required.")]
    [InlineData("", "Email is required.")]
    [InlineData("invalid-email", "Invalid email address.")]
    [InlineData("user@", "Invalid email address.")]
    [InlineData("@domain.com", "Invalid email address.")]
    public void Should_Have_Error_When_Email_Is_Invalid(string? email, string expectedMessage)
    {
        var user = CreateValidUserDto() with { Email = email! };
        var request = new CreateUserRequest(user);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Email)
            .WithErrorMessage(expectedMessage);
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("123-456-78-90")]
    [InlineData("ABC1234567")]
    public void Should_Have_Error_When_Nip_Format_Is_Invalid(string invalidNip)
    {
        var user = CreateValidUserDto() with { Nip = invalidNip };
        var request = new CreateUserRequest(user);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Nip)
            .WithErrorMessage("The Nip number must contain exactly 10 digits.");
    }

    [Fact]
    public void Should_Have_Error_When_Address_Is_Null()
    {
        var user = CreateValidUserDto() with { Address = null! };
        var request = new CreateUserRequest(user);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address)
            .WithErrorMessage("Address must be specified.");
    }

    [Fact]
    public void Should_Have_Error_When_Street_Is_Empty()
    {
        var address = CreateValidUserDto().Address with { Street = "" };
        var user = CreateValidUserDto() with { Address = address };
        var request = new CreateUserRequest(user);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address.Street)
            .WithErrorMessage("Street is required in address.");
    }

    [Fact]
    public void Should_Have_Error_When_Street_Number_Is_Empty()
    {
        var address = CreateValidUserDto().Address with { Number = "" };
        var user = CreateValidUserDto() with { Address = address };
        var request = new CreateUserRequest(user);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address.Number)
            .WithErrorMessage("Street number is required in address.");
    }

    [Fact]
    public void Should_Have_Error_When_City_Is_Empty()
    {
        var address = CreateValidUserDto().Address with { City = "" };
        var user = CreateValidUserDto() with { Address = address };
        var request = new CreateUserRequest(user);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address.City)
            .WithErrorMessage("City is required in address.");
    }

    [Theory]
    [InlineData("", "Postal code is required in address.")]
    [InlineData("00001", "Postal code must be in the format XX-XXX.")]
    [InlineData("00-00", "Postal code must be in the format XX-XXX.")]
    [InlineData("000-00", "Postal code must be in the format XX-XXX.")]
    [InlineData("XX-XXX", "Postal code must be in the format XX-XXX.")]
    public void Should_Have_Error_When_PostalCode_Is_Invalid(string postalCode, string expectedMessage)
    {
        var address = CreateValidUserDto().Address with { PostalCode = postalCode };
        var user = CreateValidUserDto() with { Address = address };
        var request = new CreateUserRequest(user);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address.PostalCode)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void Should_Have_Error_When_Country_Is_Empty()
    {
        var address = CreateValidUserDto().Address with { Country = "" };
        var user = CreateValidUserDto() with { Address = address };
        var request = new CreateUserRequest(user);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address.Country)
            .WithErrorMessage("Postal code is required in address.");
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        var request = new CreateUserRequest(CreateValidUserDto());

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}