using CreateInvoiceSystem.Abstractions.CQRS;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.DeleteInvoice;
public class DeleteInvoiceRequest(int id) : IRequest<DeleteInvoiceResponse>, ITransactionalRequest
{
    public int Id { get; } = id;
    
    [JsonIgnore]
    public int UserId { get; set; }
}
