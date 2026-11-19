using CreateInvoiceSystem.Modules.Invoices.Interfaces;

namespace CreateInvoiceSystem.Modules.Invoices.Application;

public static class InvoiceNumbering
{
    public static async Task<string> GenerateNextAsync(
        int userId, DateTime date, IInvoiceRepository invoiceRepository, CancellationToken ct)
    {
        int maxNumber = await invoiceRepository.GetMaxInvoiceNumberInMonthAsync(userId, date.Month, date.Year, ct);
        int nextNumber = maxNumber + 1;

        return $"{nextNumber}/{date.Month:00}/{date.Year}";
    }
}
