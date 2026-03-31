using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CreateInvoiceSystem.Frontend.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class MinDecimalAttribute : ValidationAttribute
{
    private readonly decimal _min;

    public MinDecimalAttribute(string min)
    {
        if (!decimal.TryParse(min, NumberStyles.Number, CultureInfo.InvariantCulture, out _min)
            && !decimal.TryParse(min.Replace('.', ','), NumberStyles.Number, CultureInfo.CurrentCulture, out _min))
        {
            throw new ArgumentException("Invalid min value for MinDecimalAttribute", nameof(min));
        }
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

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

        return parsed >= _min ? ValidationResult.Success
                             : new ValidationResult(ErrorMessage ?? $"Minimalna wartość to {_min}.");
    }
}