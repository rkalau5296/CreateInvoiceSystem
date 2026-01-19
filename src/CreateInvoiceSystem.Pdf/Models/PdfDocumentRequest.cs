namespace CreateInvoiceSystem.Pdf.Models;

public record PdfDocumentRequest(
string Title,
string Subtitle,
string ClientName,
string ClientAddress,
string ClientNip,
string UserName,
string UserAddress,
string UserNip,
List<PdfTableSection> Sections,
string FooterText);


