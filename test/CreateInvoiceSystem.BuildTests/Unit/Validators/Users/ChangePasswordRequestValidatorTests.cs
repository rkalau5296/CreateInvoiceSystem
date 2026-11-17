using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ChangePassword;
using CreateInvoiceSystem.Modules.Users.Dto;
using CreateInvoiceSystem.Modules.Users.Application.Validators;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Users;

public class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Dto_Is_Null()
    {
        var request = new ChangePasswordRequest(null!);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Dto)
            .WithErrorMessage("Dane zmiany hasła są wymagane.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_OldPassword_Is_Empty(string? invalidOldPassword)
    {
        var dto = new ChangePasswordDto(invalidOldPassword!, "Password123!", "Password123!");
        var request = new ChangePasswordRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Dto.OldPassword)
            .WithErrorMessage("Stare hasło jest wymagane.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_NewPassword_Is_Empty(string? invalidNewPassword)
    {
        var dto = new ChangePasswordDto("OldPassword123!", invalidNewPassword!, invalidNewPassword!);
        var request = new ChangePasswordRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Dto.NewPassword)
            .WithErrorMessage("Nowe hasło jest wymagane.");
    }

    [Fact]
    public void Should_Have_Error_When_NewPassword_Is_Too_Short()
    {
        var dto = new ChangePasswordDto("OldPassword123!", "12345", "12345");
        var request = new ChangePasswordRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Dto.NewPassword)
            .WithErrorMessage("Nowe hasło musi mieć co najmniej 6 znaków.");
    }

    [Fact]
    public void Should_Have_Error_When_ConfirmPassword_Does_Not_Match_NewPassword()
    {
        var dto = new ChangePasswordDto("OldPassword123!", "Password123!", "DifferentPassword123!");
        var request = new ChangePasswordRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Dto.ConfirmPassword)
            .WithErrorMessage("Nowe hasło i potwierdzenie nie są zgodne.");
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        var dto = new ChangePasswordDto("OldPassword123!", "Password123!", "Password123!");
        var request = new ChangePasswordRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}