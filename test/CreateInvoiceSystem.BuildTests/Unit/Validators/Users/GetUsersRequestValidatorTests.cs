using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;
using Xunit;

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