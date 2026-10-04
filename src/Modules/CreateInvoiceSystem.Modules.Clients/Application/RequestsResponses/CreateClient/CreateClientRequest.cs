using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Dto;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.CreateClient;
public class CreateClientRequest(CreateClientDto clientDto) : IRequest<CreateClientResponse>, ITransactionalRequest
{
    public CreateClientDto Client { get; } = clientDto;

    [JsonIgnore]
    public int UserId { get; set; } 
}
