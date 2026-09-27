using CreateInvoiceSystem.Abstractions.DecimalHelper;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.CreateProduct;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Product)
            .NotNull()
            .WithMessage("Product data cannot be null.");

        When(x => x.Product != null, () =>
        {
            RuleFor(x => x.Product.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(p => p.Product.Value)
                .NotNull().WithMessage("Value is required.")
                .GreaterThanOrEqualTo(0).WithMessage("Value must be greater than or equal to 0.")
                .Must(v => v.HasValue && DecimalHelper.GetDecimalPlaces(v.Value) <= 2)
                .WithMessage("Value must be a decimal with max 2 digits after the decimal point.");

            RuleFor(x => x.Product.Description)
                .MaximumLength(100).WithMessage("Description cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Product.Description));
        });
    }
}