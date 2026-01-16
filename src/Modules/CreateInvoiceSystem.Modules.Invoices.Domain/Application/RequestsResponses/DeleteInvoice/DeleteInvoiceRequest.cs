using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.DeleteInvoice;
public class DeleteInvoiceRequest(int id) : IRequest<DeleteInvoiceResponse>
{
    public int Id { get; } = id;
    
    [JsonIgnore]
    public int UserId { get; set; }
}
