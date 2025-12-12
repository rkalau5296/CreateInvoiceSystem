namespace CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetInvoices;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Dto;

public class GetInvoicesResponse : ResponseBase<List<InvoiceDto>>
{    
}