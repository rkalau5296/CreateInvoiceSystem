using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.UpdateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
public class UpdateClientHandler(ICommandExecutor commandExecutor, IClientRepository _clientRepository) : IRequestHandler<UpdateClientRequest, UpdateClientResponse>
{    
    public async Task<UpdateClientResponse> Handle(UpdateClientRequest request, CancellationToken cancellationToken)
    {        
        var command = new UpdateClientCommand() { Parametr = request.Client };
        
        var client = await commandExecutor.Execute(command, _clientRepository, cancellationToken);        

        return new UpdateClientResponse()
        {
            Data = client
        };
    }
}
