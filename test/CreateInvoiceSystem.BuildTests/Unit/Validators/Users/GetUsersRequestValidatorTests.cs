using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Modules.Users.Application.Validators;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Users;

public class GetUsersRequestValidatorTests
{
    private readonly GetUsersRequestValidator _validator = new();

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Validated()
    {
        var request = new GetUsersRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}