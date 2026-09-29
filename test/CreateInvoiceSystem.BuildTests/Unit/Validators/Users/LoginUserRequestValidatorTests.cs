using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.LoginUser;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Users;

public class LoginUserRequestValidatorTests
{
    private readonly LoginUserRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Dto_Is_Null()
    {
        var request = new LoginUserRequest(null!);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Dto)
            .WithErrorMessage("Dane logowania są wymagane.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Email_Is_Empty(string? invalidEmail)
    {
        var dto = new LoginUserDto(invalidEmail!, "SecretPassword123!", RememberMe: false);
        var request = new LoginUserRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Email)
            .WithErrorMessage("Adres e-mail jest wymagany.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    public void Should_Have_Error_When_Email_Format_Is_Invalid(string invalidEmail)
    {
        var dto = new LoginUserDto(invalidEmail, "SecretPassword123!", RememberMe: false);
        var request = new LoginUserRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Email)
            .WithErrorMessage("Podano nieprawidłowy format adresu e-mail.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Password_Is_Empty(string? invalidPassword)
    {
        var dto = new LoginUserDto("user@example.com", invalidPassword!, RememberMe: false);
        var request = new LoginUserRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Password)
            .WithErrorMessage("Hasło jest wymagane.");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_Not_Have_Errors_When_Request_Is_Valid(bool rememberMe)
    {
        var dto = new LoginUserDto("user@example.com", "SecretPassword123!", RememberMe: rememberMe);
        var request = new LoginUserRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}