namespace CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.DeleteInvoice;

using MediatR;

public class DeleteInvoiceRequest(int id) : IRequest<DeleteInvoiceResponse>
{
    public int Id { get; } = id;
}
