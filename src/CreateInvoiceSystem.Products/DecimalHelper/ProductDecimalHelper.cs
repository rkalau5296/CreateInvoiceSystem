using System.Globalization;

namespace CreateInvoiceSystem.Products.DecimalHelper;

public class ProductDecimalHelper
{
    public static int GetDecimalPlaces(decimal value)
    {
        string[] parts = value.ToString(CultureInfo.InvariantCulture).Split('.');
        return parts.Length == 2 ? parts[1].Length : 0;
    }
}
