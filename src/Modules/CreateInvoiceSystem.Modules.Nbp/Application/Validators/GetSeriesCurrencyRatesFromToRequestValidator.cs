using System;
using System.Linq;
using CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.PreviousDatesRates;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Nbp.Application.Validators;

public class GetSeriesCurrencyRatesFromToRequestValidator : AbstractValidator<GetSeriesCurrencyRatesFromToRequest>
{
    private static readonly string[] AllowedTables = ["A", "B", "C"];

    public GetSeriesCurrencyRatesFromToRequestValidator()
    {
        RuleFor(x => x.TableName)
            .NotEmpty().WithMessage("Table name is required.")
            .Must(table => table != null && AllowedTables.Contains(table.ToUpperInvariant()))
            .WithMessage("Table name must be 'A', 'B', or 'C'.");

        RuleFor(x => x.DateFrom)
            .LessThanOrEqualTo(x => x.DateTo)
            .WithMessage("DateFrom must be less than or equal to DateTo.");

        RuleFor(x => x.DateTo)
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("DateTo cannot be in the future.");
    }
}