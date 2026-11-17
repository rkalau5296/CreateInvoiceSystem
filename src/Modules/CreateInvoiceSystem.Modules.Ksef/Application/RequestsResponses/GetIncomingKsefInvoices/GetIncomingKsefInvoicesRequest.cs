using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetIncomingKsefInvoices;

public sealed class GetIncomingKsefInvoicesRequest : IRequest<GetIncomingKsefInvoicesResponse>
{
    public DateOnly FromDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-29);
    public DateOnly ToDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    [JsonIgnore]
    public int UserId { get; set; }
}
