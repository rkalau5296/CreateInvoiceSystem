using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.DeleteInvoice;
public class DeleteInvoiceRequest(int id) : IRequest<DeleteInvoiceResponse>
{
    public int Id { get; } = id;
}
