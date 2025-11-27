namespace CreateInvoiceSystem.Invoices.Application.RequestsResponses.GetInvoice;

using MediatR;

public class GetInvoiceRequest(int id) : IRequest<GetInvoiceResponse>
{
    public int Id { get; set; } = id;
}
