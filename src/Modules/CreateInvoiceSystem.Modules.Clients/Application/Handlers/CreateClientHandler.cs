using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Clients.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.CreateClient;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Application.Handlers;

public class CreateClientHandler(ICommandExecutor commandExecutor) : IRequestHandler<CreateClientRequest, CreateClientResponse>
{
    public async Task<CreateClientResponse> Handle(CreateClientRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateClientCommand() { Parametr = request.Client };

        var clientFromDb = await commandExecutor.Execute(command, cancellationToken);

        return new CreateClientResponse()
        {
            Data = clientFromDb
        };
    }
}