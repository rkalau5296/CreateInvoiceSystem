namespace CreateInvoiceSystem.Products.Application.Validators;

using CreateInvoiceSystem.Products.Application.RequestsResponses.CreateProduct;
using CreateInvoiceSystem.Abstractions.DecimalHelper;
using FluentValidation;


public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Product.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Product.UserId)
            .GreaterThan(0).WithMessage("UserId is required.");

        RuleFor(p => p.Product.Value)
            .NotEmpty().WithMessage("Value is required.")
            .GreaterThanOrEqualTo(0)
            .Must(v => DecimalHelper.GetDecimalPlaces(v) <= 2)
            .WithMessage("Value must be a decimal with max 2 digits after the decimal point.");
    }
}