namespace CreateInvoiceSystem.Clients.Application.RequestsResponses.UpdateClient;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class UpdateClientRequest(int id, UpdateClientDto clientDto) : IRequest<UpdateClientResponse>
{
    public UpdateClientDto Client { get; } = clientDto with { ClientId = id };
    public int Id { get; set; } = id;

}
