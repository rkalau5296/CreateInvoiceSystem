namespace CreateInvoiceSystem.Products.Application.Validators;

using CreateInvoiceSystem.Products.Application.RequestsResponses.UpdateProduct;
using CreateInvoiceSystem.Abstractions.DecimalHelper;
using FluentValidation;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Product.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100)
            .When(x => x.Product.Name != null);

        RuleFor(p => p.Product.Value)
            .NotEmpty().WithMessage("Value is required.")
            .GreaterThanOrEqualTo(0)
            .Must(v => v != null && DecimalHelper.GetDecimalPlaces(v.Value) <= 2)
            .WithMessage("Value must be a decimal with max 2 digits after the decimal point.")
            .When(x => x.Product.Value != null);

        RuleFor(x => x.Product.Description)
            .MaximumLength(100)
            .When(x => x.Product.Description != null);
    }    
}