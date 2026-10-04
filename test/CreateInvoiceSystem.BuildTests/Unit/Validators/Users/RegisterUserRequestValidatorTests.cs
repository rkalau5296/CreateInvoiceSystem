using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.RegisterUser;
using CreateInvoiceSystem.Modules.Users.Application.Validators;
using CreateInvoiceSystem.Modules.Users.Dto;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Users;

public class RegisterUserRequestValidatorTests
{
    private readonly RegisterUserRequestValidator _validator = new();

    private static RegisterUserDto CreateValidRegisterUserDto() => new()
    {
        Name = "Jan Kowalski",
        CompanyName = "Acme Corp",
        Email = "jan.kowalski@example.com",
        Password = "SecurePassword123!",
        Nip = "1234567890",
        Address = new RegisterAddressDto
        {
            Street = "Marszałkowska",
            Number = "10/2",
            City = "Warszawa",
            PostalCode = "00-001",
            Country = "Polska"
        }
    };

    [Fact]
    public void Should_Have_Error_When_User_Is_Null()
    {
        var request = new RegisterUserRequest { User = null! };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User)
            .WithErrorMessage("User data cannot be null.");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var user = CreateValidRegisterUserDto();
        user.Name = "";
        var request = new RegisterUserRequest { User = user };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Maximum_Length()
    {
        var user = CreateValidRegisterUserDto();
        user.Name = new string('a', 101);
        var request = new RegisterUserRequest { User = user };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Name);
    }

    [Fact]
    public void Should_Have_Error_When_CompanyName_Is_Empty()
    {
        var user = CreateValidRegisterUserDto();
        user.CompanyName = "";
        var request = new RegisterUserRequest { User = user };

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
        var user = CreateValidRegisterUserDto();
        user.Email = email!;
        var request = new RegisterUserRequest { User = user };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Email)
            .WithErrorMessage(expectedMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_Have_Error_When_Password_Is_Empty(string? invalidPassword)
    {
        var user = CreateValidRegisterUserDto();
        user.Password = invalidPassword!;
        var request = new RegisterUserRequest { User = user };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Password)
            .WithErrorMessage("Password is required.");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("123-456-78-90")]
    public void Should_Have_Error_When_Nip_Format_Is_Invalid(string invalidNip)
    {
        var user = CreateValidRegisterUserDto();
        user.Nip = invalidNip;
        var request = new RegisterUserRequest { User = user };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Nip)
            .WithErrorMessage("The Nip number must contain exactly 10 digits.");
    }

    [Fact]
    public void Should_Have_Error_When_Address_Is_Null()
    {
        var user = CreateValidRegisterUserDto();
        user.Address = null!;
        var request = new RegisterUserRequest { User = user };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address)
            .WithErrorMessage("Address must be specified.");
    }

    [Fact]
    public void Should_Have_Error_When_Street_Is_Empty()
    {
        var user = CreateValidRegisterUserDto();
        user.Address.Street = "";
        var request = new RegisterUserRequest { User = user };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address.Street)
            .WithErrorMessage("Street is required in address.");
    }

    [Fact]
    public void Should_Have_Error_When_Street_Number_Is_Empty()
    {
        var user = CreateValidRegisterUserDto();
        user.Address.Number = "";
        var request = new RegisterUserRequest { User = user };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address.Number)
            .WithErrorMessage("Street number is required in address.");
    }

    [Fact]
    public void Should_Have_Error_When_City_Is_Empty()
    {
        var user = CreateValidRegisterUserDto();
        user.Address.City = "";
        var request = new RegisterUserRequest { User = user };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address.City)
            .WithErrorMessage("City is required in address.");
    }

    [Theory]
    [InlineData("", "Postal code is required in address.")]
    [InlineData("00001", "Postal code must be in the format XX-XXX.")]
    [InlineData("00-00", "Postal code must be in the format XX-XXX.")]
    public void Should_Have_Error_When_PostalCode_Is_Invalid(string postalCode, string expectedMessage)
    {
        var user = CreateValidRegisterUserDto();
        user.Address.PostalCode = postalCode;
        var request = new RegisterUserRequest { User = user };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address.PostalCode)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void Should_Have_Error_When_Country_Is_Empty()
    {
        var user = CreateValidRegisterUserDto();
        user.Address.Country = "";
        var request = new RegisterUserRequest { User = user };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address.Country)
            .WithErrorMessage("Country is required in address.");
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        var request = new RegisterUserRequest { User = CreateValidRegisterUserDto() };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}