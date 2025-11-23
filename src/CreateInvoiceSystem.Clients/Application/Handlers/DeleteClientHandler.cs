namespace CreateInvoiceSystem.Clients.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Clients.Application.Commands;
using CreateInvoiceSystem.Clients.Application.Mappers;
using CreateInvoiceSystem.Clients.Application.RequestsResponses.DeleteClient;
using CreateInvoiceSystem.Abstractions.Entities;
using MediatR;

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

