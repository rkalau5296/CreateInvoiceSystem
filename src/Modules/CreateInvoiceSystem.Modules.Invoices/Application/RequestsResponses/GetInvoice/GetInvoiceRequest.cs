using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoice;

public class GetInvoiceRequest(int id) : IRequest<GetInvoiceResponse>
{
    public int Id { get; } = id;

    [JsonIgnore]
    public int? UserId { get; set; }
}