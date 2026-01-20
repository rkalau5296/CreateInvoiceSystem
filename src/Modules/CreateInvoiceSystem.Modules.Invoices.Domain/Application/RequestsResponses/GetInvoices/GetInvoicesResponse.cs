using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoices;
public class GetInvoicesResponse : ResponseBase<List<InvoiceDto>>
{    
}