using FluentValidation;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.GetUsers;

namespace CreateInvoiceSystem.Modules.Users.Application.Validators;

public class GetUsersRequestValidator : AbstractValidator<GetUsersRequest>
{
    public GetUsersRequestValidator()
    {
        
    }
}