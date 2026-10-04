using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetPdf;

public record GetInvoicePdfRequest(int InvoiceId, int UserId) : IRequest<GetInvoicePdfResponse>;
