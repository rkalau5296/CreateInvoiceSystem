using CreateInvoiceSystem.Abstractions.CQRS;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.DeleteClient;
public class DeleteClientRequest(int id) : IRequest<DeleteClientResponse>, ITransactionalRequest
{
    public int Id { get; } = id;

    [JsonIgnore]
    public int UserId { get; set; }
}