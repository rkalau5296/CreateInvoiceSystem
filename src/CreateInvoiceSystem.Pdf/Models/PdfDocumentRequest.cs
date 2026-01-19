namespace CreateInvoiceSystem.Pdf.Models;

public record PdfDocumentRequest(
string Title,
string Subtitle,
List<PdfTableSection> Sections,
string FooterText);
