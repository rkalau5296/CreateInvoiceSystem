using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClient;
public record GetClientRequest(int Id) : IRequest<GetClientResponse>
{
    public int Id { get; set; } = Id >= 1 ? Id
            : throw new ArgumentOutOfRangeException(nameof(Id), "Id must be greater than or equal to 1.");
}
