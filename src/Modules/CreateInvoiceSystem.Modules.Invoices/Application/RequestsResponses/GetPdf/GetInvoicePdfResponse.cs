namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetPdf;

public record GetInvoicePdfResponse(
byte[] PdfContent,
string InvoiceNumber,
string FileName);
