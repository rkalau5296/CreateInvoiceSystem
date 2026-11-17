namespace CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetPdf;

public record GetInvoicePdfResponse(
byte[] PdfContent,
string InvoiceNumber,
string FileName);
