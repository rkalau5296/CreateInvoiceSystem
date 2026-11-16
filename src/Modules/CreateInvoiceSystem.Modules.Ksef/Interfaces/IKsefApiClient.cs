using CreateInvoiceSystem.Modules.Ksef.Dto;

namespace CreateInvoiceSystem.Modules.Ksef.Interfaces;

public interface IKsefApiClient
{
    Task<KsefSendResult> SendInvoiceAsync(int userId, string invoiceXml, CancellationToken cancellationToken);
    Task<KsefInvoicePage> GetIncomingInvoicesAsync(
        int userId,
        DateOnly fromDate,
        DateOnly toDate,
        int pageOffset,
        int pageSize,
        CancellationToken cancellationToken);
    Task<string> DownloadInvoiceXmlAsync(int userId, string ksefNumber, CancellationToken cancellationToken);
    Task<byte[]> DownloadInvoiceXmlBytesAsync(int userId, string ksefNumber, CancellationToken cancellationToken);
}