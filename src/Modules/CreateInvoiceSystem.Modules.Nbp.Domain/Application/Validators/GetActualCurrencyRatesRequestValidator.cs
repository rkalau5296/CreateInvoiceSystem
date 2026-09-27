using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRates;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.Validators;

public class GetActualCurrencyRatesRequestValidator : AbstractValidator<GetActualCurrencyRatesRequest>
{
    private static readonly string[] AllowedTables = ["A", "B", "C"];

    public GetActualCurrencyRatesRequestValidator()
    {
        RuleFor(x => x.TableName)
            .NotEmpty().WithMessage("Table name is required.")
            .Must(table => table != null && AllowedTables.Contains(table.ToUpperInvariant()))
            .WithMessage("Table name must be 'A', 'B', or 'C'.");
    }
}