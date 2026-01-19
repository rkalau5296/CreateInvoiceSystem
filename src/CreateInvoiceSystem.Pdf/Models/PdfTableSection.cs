namespace CreateInvoiceSystem.Pdf.Models;

public record PdfTableSection(
string Name,
string[] Headers,
List<string[]> Rows);
