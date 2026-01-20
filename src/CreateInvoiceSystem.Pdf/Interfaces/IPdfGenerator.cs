using CreateInvoiceSystem.Pdf.Models;

namespace CreateInvoiceSystem.Pdf.Interfaces;

public interface IPdfGenerator
{
    byte[] Create(PdfDocumentRequest request);
}
