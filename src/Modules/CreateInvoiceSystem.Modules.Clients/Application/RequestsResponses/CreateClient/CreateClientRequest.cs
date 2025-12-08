using CreateInvoiceSystem.Modules.Clients.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.CreateClient;

public class CreateClientRequest(CreateClientDto clientDto) : IRequest<CreateClientResponse>
{
    public CreateClientDto Client { get; } = clientDto;
}
