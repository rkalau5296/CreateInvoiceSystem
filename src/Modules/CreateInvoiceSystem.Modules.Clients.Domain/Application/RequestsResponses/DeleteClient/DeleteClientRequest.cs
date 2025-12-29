using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.DeleteClient;
public class DeleteClientRequest(int id) : IRequest<DeleteClientResponse>
{
    public int Id { get; } =
        id >= 1 ? id
            : throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than or equal to 1.");
}
