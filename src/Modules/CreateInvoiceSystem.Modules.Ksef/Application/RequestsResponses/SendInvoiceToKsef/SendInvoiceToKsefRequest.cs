using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.SendInvoiceToKsef;

public sealed class SendInvoiceToKsefRequest(int invoiceId) : IRequest<int>
{
    public int InvoiceId { get; } = invoiceId;

    [JsonIgnore]
    public int UserId { get; set; }
}
