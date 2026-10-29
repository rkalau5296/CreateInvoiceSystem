namespace CreateInvoiceSystem.Modules.Ksef.Interfaces;

public interface IKsefInvoiceXmlGenerator
{
    Task<string> GenerateXmlAsync(int? userId, int invoiceId, CancellationToken cancellationToken);
}