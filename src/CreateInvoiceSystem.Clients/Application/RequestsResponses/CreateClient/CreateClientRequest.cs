namespace CreateInvoiceSystem.Clients.Application.RequestsResponses.CreateClient;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class CreateClientRequest(ClientDto clientDto) : IRequest<CreateClientResponse>
{
    public ClientDto Client { get; } = clientDto;
}
