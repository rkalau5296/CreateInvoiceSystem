namespace CreateInvoiceSystem.Products.Application.Validators;

using CreateInvoiceSystem.Products.Application.RequestsResponses.UpdateProduct;
using FluentValidation;
using System;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Product.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(p => p.Product.Value)
            .NotEmpty().WithMessage("Value is required.")
            .GreaterThanOrEqualTo(0)
            .Must(v => DecimalPlaces(v) <= 2)
            .WithMessage("Value must be a decimal with max 2 digits after the decimal point.");

        //RuleFor(x => x.UserId)
        //    .GreaterThan(0).WithMessage("UserId is required.");

    }

    private int DecimalPlaces(decimal dec)
    {
        dec = Math.Abs(dec);
        dec -= (int)dec;
        int places = 0;
        while (dec != 0)
        {
            dec *= 10;
            dec -= (int)dec;
            places++;
            if (places > 10) break;
        }
        return places;
    }
}