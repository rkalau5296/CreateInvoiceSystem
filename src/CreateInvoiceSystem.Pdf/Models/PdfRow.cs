namespace CreateInvoiceSystem.Pdf.Models;

public record PdfRow(
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
