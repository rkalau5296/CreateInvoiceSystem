using CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.PreviousDatesRate;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Nbp.Application.Validators;

public class GetSeriesCurrencyRateFromToRequestValidator : AbstractValidator<GetSeriesCurrencyRateFromToRequest>
{
    private static readonly string[] AllowedTables = ["A", "B", "C"];

    public GetSeriesCurrencyRateFromToRequestValidator()
    {
        RuleFor(x => x.TableName)
            .NotEmpty().WithMessage("Table name is required.")
            .Must(table => table != null && AllowedTables.Contains(table.ToUpperInvariant()))
            .WithMessage("Table name must be 'A', 'B', or 'C'.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required.")
            .Length(3).WithMessage("Currency code must be exactly 3 characters long.")
            .Matches(@"^[a-zA-Z]{3}$").WithMessage("Currency code must consist of 3 letters.");

        RuleFor(x => x.DateFrom)
            .LessThanOrEqualTo(x => x.DateTo)
            .WithMessage("DateFrom must be less than or equal to DateTo.");

        RuleFor(x => x.DateTo)
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("DateTo cannot be in the future.");
    }
}