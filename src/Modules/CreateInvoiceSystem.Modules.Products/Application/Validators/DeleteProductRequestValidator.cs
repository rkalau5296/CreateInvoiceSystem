using CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.DeleteProduct;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Products.Application.Validators;

public class DeleteProductRequestValidator : AbstractValidator<DeleteProductRequest>
{
    public DeleteProductRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Id must be greater than or equal to 1.");
    }
}