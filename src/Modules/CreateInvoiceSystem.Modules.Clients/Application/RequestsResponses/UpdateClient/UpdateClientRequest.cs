using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Dto;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.UpdateClient;

public class UpdateClientRequest(UpdateClientDto clientDto, int id) : IRequest<UpdateClientResponse>, ITransactionalRequest
{
    public UpdateClientDto Client { get; } = clientDto != null ? clientDto with { ClientId = id } : null!;

    public int Id { get; } = id;

    [JsonIgnore]
    public int UserId { get; set; }
}