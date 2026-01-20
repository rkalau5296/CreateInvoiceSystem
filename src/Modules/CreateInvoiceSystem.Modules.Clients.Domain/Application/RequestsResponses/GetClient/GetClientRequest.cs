using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClient;
public class GetClientRequest : IRequest<GetClientResponse>
{
    private int _id;
    public int Id
    {
        get => _id;    
        set => _id = value >= 1 ? value
               : throw new ArgumentOutOfRangeException(nameof(Id), "Id must be greater than or equal to 1.");
    }
    [JsonIgnore]
    public int? UserId { get; set; }
    
    public GetClientRequest(int id)
    {
        Id = id;
    }
    
    public GetClientRequest() { }
}
