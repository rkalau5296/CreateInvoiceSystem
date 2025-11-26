namespace CreateInvoiceSystem.Clients.Application.RequestsResponses.UpdateClient;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class UpdateClientRequest(int id, ClientDto clientDto) : IRequest<UpdateClientResponse>
{
    public ClientDto Client { get; } = clientDto with { ClientId = id };
    public int Id { get; set; } = id;

}
