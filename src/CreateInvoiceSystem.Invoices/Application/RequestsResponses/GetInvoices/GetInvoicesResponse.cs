namespace CreateInvoiceSystem.Invoices.Application.RequestsResponses.GetInvoices;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.Dto;

public class GetInvoicesResponse : ResponseBase<List<InvoiceDto>>
{    
}