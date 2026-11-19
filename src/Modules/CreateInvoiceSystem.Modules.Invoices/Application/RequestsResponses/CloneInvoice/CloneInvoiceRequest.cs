using CreateInvoiceSystem.Abstractions.CQRS;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.CloneInvoice;

public class CloneInvoiceRequest(int id) : IRequest<CloneInvoiceResponse>, ITransactionalRequest
{
    public int Id { get; } = id;

    [JsonIgnore]
    public int UserId { get; set; }
}
