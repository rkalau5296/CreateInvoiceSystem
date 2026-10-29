using CreateInvoiceSystem.API.Mappers.KsefMapper;
using CreateInvoiceSystem.Modules.Invoices.Interfaces;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using CreateInvoiceSystem.Modules.Ksef.Mappers;

namespace CreateInvoiceSystem.API.Adapters.KsefInvoiceXmlAdapter;

public class KsefInvoiceXmlAdapter(IInvoiceRepository invoiceRepository) : IKsefInvoiceXmlGenerator
{
    public async Task<string> GenerateXmlAsync(int? userId, int invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetInvoiceByIdAsync(userId, invoiceId, cancellationToken);

        var request = invoice.ToKsefInvoiceXmlRequest();
        var xml = InvoiceToFa3XmlMapper.MapToFa3Xml(request);

        return xml.ToString();
    }
}