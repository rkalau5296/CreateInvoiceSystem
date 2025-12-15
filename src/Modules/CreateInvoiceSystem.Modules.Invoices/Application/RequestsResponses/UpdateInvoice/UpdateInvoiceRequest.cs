namespace CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.UpdateInvoice;

using CreateInvoiceSystem.Modules.Invoices.Dto;
using MediatR;

public class UpdateInvoiceRequest(int id, UpdateInvoiceDto updateInvoiceDto) : IRequest<UpdateInvoiceResponse>
{
    public UpdateInvoiceDto Invoice { get; } = updateInvoiceDto with { InvoiceId = id };
    public int Id { get; set; } = id;

}
