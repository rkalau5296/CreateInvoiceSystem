namespace CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.CreateInvoice;

using CreateInvoiceSystem.Modules.Invoices.Dto;
using MediatR;

public class CreateInvoiceRequest(CreateInvoiceDto invoiceDto) : IRequest<CreateInvoiceResponse>
{
    public CreateInvoiceDto Invoice { get; } = invoiceDto;
}
