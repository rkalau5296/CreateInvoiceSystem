using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoices;
public class GetInvoicesRequest : IRequest<GetInvoicesResponse>
{
    public int? UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
