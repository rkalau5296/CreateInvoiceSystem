using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Dto;

namespace CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetInvoices;
public class GetInvoicesResponse : ResponseBase<List<InvoiceDto>>
{
    public int TotalCount { get; set; }
}