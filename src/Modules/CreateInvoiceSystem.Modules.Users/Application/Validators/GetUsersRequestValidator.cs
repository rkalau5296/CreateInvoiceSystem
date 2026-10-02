using FluentValidation;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUsers;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;

public class GetUsersRequestValidator : AbstractValidator<GetUsersRequest>
{
    public GetUsersRequestValidator()
    {
        
    }
}