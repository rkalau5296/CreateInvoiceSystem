using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.CreateInvoice;



public class CreateInvoiceRequest(CreateInvoiceDto invoiceDto) : IRequest<CreateInvoiceResponse>
{
    public CreateInvoiceDto Invoice { get; } = invoiceDto;
    
    [JsonIgnore]
    public int UserId { get; set; }
}
