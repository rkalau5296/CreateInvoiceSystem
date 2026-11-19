namespace CreateInvoiceSystem.Modules.Ksef.Dto;

public static class KsefRejectionClassifier
{
    public static bool IsConfirmed(int? failedCount, IReadOnlyList<int?> invoiceStatusCodes) =>
        failedCount == 1 && invoiceStatusCodes.Count == 1 &&
        invoiceStatusCodes[0] is 410 or 415 or 430 or 450;
}
