using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClients;
public class GetClientsRequest : IRequest<GetClientsResponse>
{
    [JsonIgnore]
    public int? UserId { get; set; }
}
