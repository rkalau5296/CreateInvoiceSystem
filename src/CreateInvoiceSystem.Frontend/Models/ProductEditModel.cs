using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CreateInvoiceSystem.Frontend.Models;

public class ProductEditModel : IValidatableObject
{
    public int ProductId { get; set; }
    public int UserId { get; set; }

    [Required(ErrorMessage = "Nazwa jest wymagana.")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cena jest wymagana.")]
    public string ValueString { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        var s = (ValueString ?? string.Empty).Trim().Replace(',', '.');
        if (!decimal.TryParse(s, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var val))
        {
            results.Add(new ValidationResult("Nieprawidłowa wartość ceny.", new[] { nameof(ValueString) }));
            return results;
        }

        if (val < 0m)
        {
            results.Add(new ValidationResult("Cena nie może być ujemna.", new[] { nameof(ValueString) }));
        }

        return results;
    }
}