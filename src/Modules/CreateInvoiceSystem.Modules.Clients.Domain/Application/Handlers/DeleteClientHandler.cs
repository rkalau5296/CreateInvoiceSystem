using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.DeleteClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
public class DeleteClientHandler(ICommandExecutor commandExecutor, IClientRepository _clientRepository) : IRequestHandler<DeleteClientRequest, DeleteClientResponse>
{
    public async Task<DeleteClientResponse> Handle(DeleteClientRequest request, CancellationToken cancellationToken)
    {
        var client = new Client { ClientId = request.Id };

        var command = new DeleteClientCommand() { Parametr = client };
        var clientDto = await commandExecutor.Execute(command, _clientRepository, cancellationToken);

        return new DeleteClientResponse()
        {
            Data = clientDto
        };
    }
}