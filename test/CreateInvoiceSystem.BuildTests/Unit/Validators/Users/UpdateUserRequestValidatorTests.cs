using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.UpdateUser;
using CreateInvoiceSystem.Modules.Users.Dto;
using CreateInvoiceSystem.Modules.Users.Application.Validators;


namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Users;

public class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _validator = new();

    private static UpdateUserDto CreateValidUpdateUserDto() => new(
        UserId: 1,
        Name: "Jan Kowalski",
        CompanyName: "Acme Corp",
        Email: "jan.kowalski@example.com",
        Nip: "1234567890",
        BankAccountNumber: "12345678901234567890123456",
        Address: new UpdateAddressDto(
            Street: "Marszałkowska",
            Number: "10/2",
            City: "Warszawa",
            PostalCode: "00-001",
            Country: "Polska"
        )
    );

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Have_Error_When_Id_Is_Invalid(int invalidId)
    {
        var request = new UpdateUserRequest(CreateValidUpdateUserDto(), invalidId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Id must be greater than or equal to 1.");
    }

    [Fact]
    public void Should_Have_Error_When_User_Is_Null()
    {
        var request = new UpdateUserRequest(null!, 1);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User)
            .WithErrorMessage("User data cannot be null.");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var user = CreateValidUpdateUserDto() with { Name = "" };
        var request = new UpdateUserRequest(user, 1);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Maximum_Length()
    {
        var user = CreateValidUpdateUserDto() with { Name = new string('a', 101) };
        var request = new UpdateUserRequest(user, 1);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Name);
    }

    [Fact]
    public void Should_Have_Error_When_CompanyName_Is_Empty()
    {
        var user = CreateValidUpdateUserDto() with { CompanyName = "" };
        var request = new UpdateUserRequest(user, 1);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.CompanyName)
            .WithErrorMessage("Company name is required.");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("123-456-78-90")]
    public void Should_Have_Error_When_Nip_Format_Is_Invalid(string invalidNip)
    {
        var user = CreateValidUpdateUserDto() with { Nip = invalidNip };
        var request = new UpdateUserRequest(user, 1);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Nip)
            .WithErrorMessage("The Nip number must contain exactly 10 digits.");
    }

    [Theory]
    [InlineData("", "Email is required.")]
    [InlineData("invalid-email", "Invalid email address.")]
    [InlineData("user@", "Invalid email address.")]
    public void Should_Have_Error_When_Email_Is_Invalid(string email, string expectedMessage)
    {
        var user = CreateValidUpdateUserDto() with { Email = email };
        var request = new UpdateUserRequest(user, 1);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Email)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void Should_Have_Error_When_Street_Is_Empty()
    {
        var address = CreateValidUpdateUserDto().Address! with { Street = "" };
        var user = CreateValidUpdateUserDto() with { Address = address };
        var request = new UpdateUserRequest(user, 1);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address!.Street)
            .WithErrorMessage("Street is required in address.");
    }

    [Fact]
    public void Should_Have_Error_When_Street_Number_Is_Empty()
    {
        var address = CreateValidUpdateUserDto().Address! with { Number = "" };
        var user = CreateValidUpdateUserDto() with { Address = address };
        var request = new UpdateUserRequest(user, 1);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address!.Number)
            .WithErrorMessage("Street number is required in address.");
    }

    [Fact]
    public void Should_Have_Error_When_City_Is_Empty()
    {
        var address = CreateValidUpdateUserDto().Address! with { City = "" };
        var user = CreateValidUpdateUserDto() with { Address = address };
        var request = new UpdateUserRequest(user, 1);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address!.City)
            .WithErrorMessage("City is required in address.");
    }

    [Theory]
    [InlineData("", "Postal code is required in address.")]
    [InlineData("00001", "Postal code must be in the format XX-XXX.")]
    [InlineData("00-00", "Postal code must be in the format XX-XXX.")]
    public void Should_Have_Error_When_PostalCode_Is_Invalid(string postalCode, string expectedMessage)
    {
        var address = CreateValidUpdateUserDto().Address! with { PostalCode = postalCode };
        var user = CreateValidUpdateUserDto() with { Address = address };
        var request = new UpdateUserRequest(user, 1);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address!.PostalCode)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void Should_Have_Error_When_Country_Is_Empty()
    {
        var address = CreateValidUpdateUserDto().Address! with { Country = "" };
        var user = CreateValidUpdateUserDto() with { Address = address };
        var request = new UpdateUserRequest(user, 1);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.User.Address!.Country)
            .WithErrorMessage("Country is required in address.");
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Optional_Fields_And_Address_Are_Null()
    {
        var user = new UpdateUserDto(
            UserId: 1,
            Name: null!,
            CompanyName: null!,
            Email: null!,
            Nip: null!,
            BankAccountNumber: null!,
            Address: null!
        );
        var request = new UpdateUserRequest(user, 1);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        var request = new UpdateUserRequest(CreateValidUpdateUserDto(), 1);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}