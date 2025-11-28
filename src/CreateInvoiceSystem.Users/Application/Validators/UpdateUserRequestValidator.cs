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

        RuleFor(x => x.User.Nip)           
           .Matches(@"^\d{10}$")
           .WithMessage("The Nip number must contain exactly 10 digits.");

        RuleFor(p => p.User.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
            .WithMessage("Invalid email address.");   
        
        When(x => x.User.AddressDto != null, () =>
        {
            RuleFor(x => x.User.AddressDto.Street)
                .NotEmpty().WithMessage("Street is required in address.");

            RuleFor(x => x.User.AddressDto.Number)
                .NotEmpty().WithMessage("Street number is required in address.");

            RuleFor(x => x.User.AddressDto.City)
                .NotEmpty().WithMessage("City is required in address.");

            RuleFor(x => x.User.AddressDto.PostalCode)
                .NotEmpty().WithMessage("Postal code is required in address.")
                .Matches(@"^\d{2}-\d{3}$")
                .WithMessage("Postal code must be in the format XX-XXX.");            

            RuleFor(x => x.User.AddressDto.Country)
                .NotEmpty().WithMessage("Postal code is required in address.");
        });
    }
}