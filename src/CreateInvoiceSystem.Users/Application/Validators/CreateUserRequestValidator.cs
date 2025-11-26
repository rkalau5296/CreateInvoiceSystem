namespace CreateInvoiceSystem.Users.Application.Validators;

using CreateInvoiceSystem.Users.Application.RequestsResponses;
using CreateInvoiceSystem.Users.Application.RequestsResponses.CreateUser;
using FluentValidation;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
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