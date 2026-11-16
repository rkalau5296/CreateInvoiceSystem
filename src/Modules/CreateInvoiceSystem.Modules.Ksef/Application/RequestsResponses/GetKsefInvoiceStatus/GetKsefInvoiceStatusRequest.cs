using CreateInvoiceSystem.Modules.Ksef.Dto;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceStatus;

public sealed class GetKsefInvoiceStatusRequest(int invoiceId) : IRequest<KsefOutboxStatus?>
{
    public int InvoiceId { get; } = invoiceId;

    [JsonIgnore]
    public int UserId { get; set; }
}
