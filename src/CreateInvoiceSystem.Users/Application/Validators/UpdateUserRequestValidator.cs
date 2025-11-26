namespace CreateInvoiceSystem.Users.Application.Validators;

using CreateInvoiceSystem.Users.Application.RequestsResponses.UpdateUser;
using FluentValidation;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.User.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(p => p.User.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
            .WithMessage("Invalid email address.");


        //RuleFor(x => x.UserId)
        //    .GreaterThan(0).WithMessage("UserId is required.");

    }
}