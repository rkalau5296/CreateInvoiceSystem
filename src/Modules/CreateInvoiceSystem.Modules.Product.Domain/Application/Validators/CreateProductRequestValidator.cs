using CreateInvoiceSystem.Abstractions.DecimalHelper;
using FluentValidation;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.CreateProduct;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Validators;
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
            .Must(v => v != null && DecimalHelper.GetDecimalPlaces(v.Value) <= 2)
            .WithMessage("Value must be a decimal with max 2 digits after the decimal point.");            
    }
}