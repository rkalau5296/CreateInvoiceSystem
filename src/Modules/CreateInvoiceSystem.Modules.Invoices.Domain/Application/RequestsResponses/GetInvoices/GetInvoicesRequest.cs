using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoices;
public class GetInvoicesRequest : IRequest<GetInvoicesResponse>
{
    public int? UserId { get; set; }
}
