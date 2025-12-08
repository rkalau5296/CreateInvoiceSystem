using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Clients.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.DeleteClient;
using CreateInvoiceSystem.Modules.Clients.Entities;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Application.Handlers;

public class DeleteClientHandler(ICommandExecutor commandExecutor) : IRequestHandler<DeleteClientRequest, DeleteClientResponse>
{
    public async Task<DeleteClientResponse> Handle(DeleteClientRequest request, CancellationToken cancellationToken)
    {
        var client = new Client { ClientId = request.Id };

        var command = new DeleteClientCommand { Parametr = client };
        await commandExecutor.Execute(command, cancellationToken);

        return new DeleteClientResponse()
        {
            Data = ClientMappers.ToDto(client)
        };
    }
}

