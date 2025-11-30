namespace CreateInvoiceSystem.Invoices.Application.RequestsResponses.CreateInvoice;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class CreateInvoiceRequest(CreateInvoiceDto invoiceDto) : IRequest<CreateInvoiceResponse>
{
    public CreateInvoiceDto Invoice { get; } = invoiceDto;
}
