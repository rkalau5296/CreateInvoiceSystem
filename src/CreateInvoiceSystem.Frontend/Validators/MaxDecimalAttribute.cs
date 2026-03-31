using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CreateInvoiceSystem.Frontend.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class MaxDecimalAttribute : ValidationAttribute
{
    private readonly decimal _max;

    public MaxDecimalAttribute(string max)
    {
        if (!decimal.TryParse(max, NumberStyles.Number, CultureInfo.InvariantCulture, out _max)
            && !decimal.TryParse(max.Replace('.', ','), NumberStyles.Number, CultureInfo.CurrentCulture, out _max))
        {
            throw new ArgumentException("Invalid max value for MaxDecimalAttribute", nameof(max));
        }
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        decimal parsed;
        if (value is decimal d) parsed = d;
        else
        {
            var s = Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
            if (!decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out parsed)
                && !decimal.TryParse(s.Replace(',', '.'), NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out parsed))
            {
                return new ValidationResult(ErrorMessage ?? "Pole musi być liczbą.");
            }
        }

        return parsed <= _max ? ValidationResult.Success
                              : new ValidationResult(ErrorMessage ?? $"Maksymalna wartość to {_max}.");
    }
}