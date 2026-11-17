namespace CreateInvoiceSystem.Modules.Ksef.Interfaces;

public interface IKsefInvoiceBytesClient
{
    Task<byte[]> DownloadAsync(string ksefNumber, string accessToken, CancellationToken cancellationToken);
}
