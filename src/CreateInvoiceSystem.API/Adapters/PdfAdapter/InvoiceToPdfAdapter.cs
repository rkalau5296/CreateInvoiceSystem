using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Pdf.Interfaces;
using CreateInvoiceSystem.Pdf.Models;

namespace CreateInvoiceSystem.API.Adapters.PdfAdapter
{
    public class InvoiceToPdfAdapter(IPdfGenerator pdfGenerator) : IInvoiceExportService
    {
        public byte[] ExportToPdf(InvoiceDto invoice)
        {            
            var pdfRequest = new PdfDocumentRequest(
                Title: $"Faktura {invoice.Title}",
                Subtitle: $"Data: {invoice.CreatedDate:d}",
                Sections: new List<PdfTableSection>
                {
                new PdfTableSection(
                    "Pozycje faktury",
                    new[] { "Nazwa", "Ilość", "Cena" },
                    invoice.InvoicePositions.Select(p => new[]
                    {
                        p.ProductName,
                        p.Quantity.ToString(),
                        $"{p.ProductValue} PLN"
                    }).ToList()
                )
                },
                FooterText: "Dziękujemy za zakupy!"
            );

            return pdfGenerator.Create(pdfRequest);
        }        
    }
}
