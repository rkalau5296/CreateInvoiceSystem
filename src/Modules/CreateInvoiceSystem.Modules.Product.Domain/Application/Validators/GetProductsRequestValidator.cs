using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Validators;

public class GetProductsRequestValidator : AbstractValidator<GetProductsRequest>
{
    public GetProductsRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 1000)
            .WithMessage("Page size must be between 1 and 1000.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("Search term cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}