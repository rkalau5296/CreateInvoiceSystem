using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.DeleteUser;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Users.Application.Validators
{
    public class DeleteUserRequestValidator : AbstractValidator<DeleteUserRequest>
    {
        public DeleteUserRequestValidator()
        {
            RuleFor(x => x.Id).GreaterThanOrEqualTo(1);
        }
    }
}