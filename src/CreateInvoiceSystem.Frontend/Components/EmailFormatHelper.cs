using System.Text.RegularExpressions;

namespace CreateInvoiceSystem.Frontend.Components
{
    public static class EmailFormatHelper
    {
        public static bool IsValidOrEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ||
            Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
