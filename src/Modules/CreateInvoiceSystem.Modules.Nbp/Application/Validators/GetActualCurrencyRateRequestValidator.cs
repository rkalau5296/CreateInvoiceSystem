using CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.ActualRate;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Nbp.Application.Validators;

public class GetActualCurrencyRateRequestValidator : AbstractValidator<GetActualCurrencyRateRequest>
{
    private static readonly string[] AllowedTables = ["A", "B", "C"];

    public GetActualCurrencyRateRequestValidator()
    {
        RuleFor(x => x.TableName)
            .NotEmpty().WithMessage("Table name is required.")
            .Must(table => table != null && AllowedTables.Contains(table.ToUpperInvariant()))
            .WithMessage("Table name must be 'A', 'B', or 'C'.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required.")
            .Length(3).WithMessage("Currency code must be exactly 3 characters long.")
            .Matches(@"^[a-zA-Z]{3}$").WithMessage("Currency code must consist of 3 letters.");
    }
}