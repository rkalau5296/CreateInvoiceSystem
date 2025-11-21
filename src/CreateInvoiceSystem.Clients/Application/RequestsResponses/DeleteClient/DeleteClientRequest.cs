namespace CreateInvoiceSystem.Clients.Application.RequestsResponses.DeleteClient;

using MediatR;

public class DeleteClientRequest(int id) : IRequest<DeleteClientResponse>
{
    public int Id { get; } = id;
}
