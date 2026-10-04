using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.GetClient;
public class GetClientRequest(int id) : IRequest<GetClientResponse>
{
    public int Id { get; } = id;

    [JsonIgnore]
    public int UserId { get; set; }  
}
