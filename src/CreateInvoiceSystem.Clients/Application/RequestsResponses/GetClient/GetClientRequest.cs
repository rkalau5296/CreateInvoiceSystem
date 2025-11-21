namespace CreateInvoiceSystem.Clients.Application.RequestsResponses.GetClient;

using MediatR;

public record GetClientRequest(int Id) : IRequest<GetClientResponse>
{
    public int Id { get; set; } = Id;
}
