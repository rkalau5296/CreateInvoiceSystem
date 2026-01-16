using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.UpdateInvoice;
public class UpdateInvoiceRequest(int id, UpdateInvoiceDto updateInvoiceDto) : IRequest<UpdateInvoiceResponse>
{
    public UpdateInvoiceDto Invoice { get; } = updateInvoiceDto with { InvoiceId = id };
    public int Id { get; set; } = id;
    
    [JsonIgnore]
    public int UserId { get; set; }

}
