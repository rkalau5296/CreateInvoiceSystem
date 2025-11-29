namespace CreateInvoiceSystem.Clients.Application.RequestsResponses.GetClient;

using MediatR;

public record GetClientRequest(int id) : IRequest<GetClientResponse>
{
    public int Id { get; set; } = id >= 1 ? id
            : throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than or equal to 1.");
}
