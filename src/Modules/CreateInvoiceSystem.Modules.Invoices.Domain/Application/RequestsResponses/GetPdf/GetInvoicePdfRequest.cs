using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetPdf;

public record GetInvoicePdfRequest(int InvoiceId, int UserId) : IRequest<GetInvoicePdfResponse>;
