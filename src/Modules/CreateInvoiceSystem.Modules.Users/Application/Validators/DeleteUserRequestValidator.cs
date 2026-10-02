using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.DeleteUser;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Validators
{
    public class DeleteUserRequestValidator : AbstractValidator<DeleteUserRequest>
    {
        public DeleteUserRequestValidator()
        {
            RuleFor(x => x.Id).GreaterThanOrEqualTo(1);
        }
    }
}