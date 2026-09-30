using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProduct;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Validators;

public class GetProductRequestValidator : AbstractValidator<GetProductRequest>
{
    public GetProductRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Id must be greater than or equal to 1.");
    }
}