using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.CreateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
public class CreateClientHandler(ICommandExecutor commandExecutor, IClientRepository _clientRepository) : IRequestHandler<CreateClientRequest, CreateClientResponse>
{
    public async Task<CreateClientResponse> Handle(CreateClientRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateClientCommand() { Parametr = request.Client };

        var clientFromDb = await commandExecutor.Execute(command, _clientRepository, cancellationToken);

        return new CreateClientResponse()
        {
            Data = clientFromDb
        };
    }
}