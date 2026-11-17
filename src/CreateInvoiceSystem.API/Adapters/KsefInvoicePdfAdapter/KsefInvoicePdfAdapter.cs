using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using CreateInvoiceSystem.Pdf;

namespace CreateInvoiceSystem.API.Adapters.KsefInvoicePdfAdapter;

public sealed class KsefInvoicePdfAdapter(KsefInvoicePdfGenerator generator) : IKsefInvoicePdfGenerator
{
    public byte[] Create(byte[] xml, string ksefNumber, string verificationUrl) =>
        generator.Create(xml, ksefNumber, verificationUrl);
}
