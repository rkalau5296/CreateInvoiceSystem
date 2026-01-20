using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetPdf;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers
{
    public class GetInvoicePdfHandler(
    IQueryExecutor queryExecutor,
    IInvoiceRepository invoiceRepository,
    IInvoiceExportService exportService) : IRequestHandler<GetInvoicePdfRequest, GetInvoicePdfResponse>
    {
        public async Task<GetInvoicePdfResponse> Handle(GetInvoicePdfRequest request, CancellationToken cancellationToken)
        {
            GetInvoiceQuery query = new(request.UserId, request.InvoiceId);
            var invoice = await queryExecutor.Execute(query, invoiceRepository, cancellationToken);

            if (invoice == null)
            {
                return null;
            }

            var invoiceDto = InvoiceMappers.ToDto(invoice);

            var pdfBytes = await exportService.ExportToPdfAsync(invoiceDto, request.UserId, cancellationToken);

            return new GetInvoicePdfResponse(
                PdfContent: pdfBytes,
                InvoiceNumber: invoiceDto.Title,
                FileName: $"Faktura_{invoiceDto.Title.Replace("/", "_")}.pdf"
            );
        }
    }
}
