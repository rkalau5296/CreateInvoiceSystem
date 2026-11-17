using MediatR;

namespace CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoicePdf;

public sealed record GetKsefInvoicePdfRequest(int UserId, string KsefNumber) : IRequest<byte[]>;
