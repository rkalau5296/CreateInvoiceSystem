namespace CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.UpdateInvoice;

using CreateInvoiceSystem.Modules.Invoices.Dto;
using MediatR;

public class UpdateInvoiceRequest(int id, InvoiceDto invoiceDto) : IRequest<UpdateInvoiceResponse>
{
    public InvoiceDto Invoice { get; } = invoiceDto with { InvoiceId = id };
    public int Id { get; set; } = id;

}
