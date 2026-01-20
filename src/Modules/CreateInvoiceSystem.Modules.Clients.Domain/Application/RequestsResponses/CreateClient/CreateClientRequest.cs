using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.CreateClient;
public class CreateClientRequest(CreateClientDto clientDto) : IRequest<CreateClientResponse>
{
    public CreateClientDto Client { get; } = clientDto;

    [JsonIgnore]
    public int UserId { get; set; } 
}
