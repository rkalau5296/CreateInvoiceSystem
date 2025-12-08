using CreateInvoiceSystem.Modules.Clients.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.CreateClient;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Application.Handlers;

public class CreateClientHandler : IRequestHandler<CreateClientRequest, CreateClientResponse>
{
    private CreateClientCommand _command;

    public CreateClientHandler(CreateClientCommand command)
    {
        _command = command;
    }

    public async Task<CreateClientResponse> Handle(CreateClientRequest request, CancellationToken cancellationToken)
    {
        var clientFromDb = await _command.Execute(request.Client, cancellationToken);

        return new CreateClientResponse()
        {
            Data = clientFromDb
        };
    }
}