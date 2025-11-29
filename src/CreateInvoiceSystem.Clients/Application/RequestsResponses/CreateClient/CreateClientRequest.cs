namespace CreateInvoiceSystem.Clients.Application.RequestsResponses.CreateClient;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class CreateClientRequest(CreateClientDto clientDto) : IRequest<CreateClientResponse>
{
    public CreateClientDto Client { get; } = clientDto;
}
