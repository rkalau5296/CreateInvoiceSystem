using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoice;
public class GetInvoiceRequest(int id) : IRequest<GetInvoiceResponse>
{
    public int Id { get; set; } = id;
}
