using CreateInvoiceSystem.Modules.Ksef.Dto;

namespace CreateInvoiceSystem.Modules.Ksef.Interfaces;

public interface IKsefApiClient
{
    Task<string> AuthenticateAsync(CancellationToken cancellationToken);
    Task<KsefSendResult> SendInvoiceAsync(string invoiceXml, CancellationToken cancellationToken);
    Task<KsefInvoicePage> GetIncomingInvoicesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int pageOffset,
        int pageSize,
        CancellationToken cancellationToken);
    Task<string> DownloadInvoiceXmlAsync(string ksefNumber, CancellationToken cancellationToken);
}