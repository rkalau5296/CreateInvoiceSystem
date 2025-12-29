using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.CreateClient;
public class CreateClientRequest(CreateClientDto clientDto) : IRequest<CreateClientResponse>
{
    public CreateClientDto Client { get; } = clientDto;
}
