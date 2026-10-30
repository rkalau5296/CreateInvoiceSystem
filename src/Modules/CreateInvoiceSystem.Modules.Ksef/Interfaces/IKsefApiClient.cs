using CreateInvoiceSystem.Modules.Ksef.Dto;

namespace CreateInvoiceSystem.Modules.Ksef.Interfaces;

public interface IKsefApiClient
{
    Task<string> AuthenticateAsync(CancellationToken cancellationToken);
    Task<KsefSendResult> SendInvoiceAsync(string invoiceXml, CancellationToken cancellationToken);

}