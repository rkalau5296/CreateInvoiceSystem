using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetPdf;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers
{
    public class GetInvoicePdfHandler(IInvoiceRepository _invoiceRepository, IInvoiceExportService exportService) 
        : IRequestHandler<GetInvoicePdfRequest, GetInvoicePdfResponse>
    {
        public async Task<GetInvoicePdfResponse> Handle(GetInvoicePdfRequest request, CancellationToken cancellationToken)
        {            
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(request.UserId, request.InvoiceId, cancellationToken)
               ?? throw new InvalidOperationException($"Invoice with ID {request.InvoiceId} not found.");

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
