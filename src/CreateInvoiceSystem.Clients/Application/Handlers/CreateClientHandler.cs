namespace CreateInvoiceSystem.Clients.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Clients.Application.Commands;
using CreateInvoiceSystem.Clients.Application.RequestsResponses.CreateClient;
using MediatR;

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