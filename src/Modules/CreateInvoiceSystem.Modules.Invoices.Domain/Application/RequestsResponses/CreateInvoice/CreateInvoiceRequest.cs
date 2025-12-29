using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.CreateInvoice;



public class CreateInvoiceRequest(CreateInvoiceDto invoiceDto) : IRequest<CreateInvoiceResponse>
{
    public CreateInvoiceDto Invoice { get; } = invoiceDto;
}
