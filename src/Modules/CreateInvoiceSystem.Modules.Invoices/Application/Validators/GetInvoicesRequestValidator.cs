using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetInvoices;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Invoices.Application.Validators;

public class GetInvoicesRequestValidator : AbstractValidator<GetInvoicesRequest>
{
    public GetInvoicesRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("Search term cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}