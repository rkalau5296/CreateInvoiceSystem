namespace CreateInvoiceSystem.Invoices.Application.RequestsResponses.CreateInvoice;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class CreateInvoiceRequest(InvoiceDto invoiceDto) : IRequest<CreateInvoiceResponse>
{
    public InvoiceDto Invoice { get; } = invoiceDto;
}
