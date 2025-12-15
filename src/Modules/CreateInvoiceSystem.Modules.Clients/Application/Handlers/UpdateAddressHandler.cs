using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Clients.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.UpdateClient;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Application.Handlers;

public class UpdateClientHandler(ICommandExecutor commandExecutor) : IRequestHandler<UpdateClientRequest, UpdateClientResponse>
{    
    public async Task<UpdateClientResponse> Handle(UpdateClientRequest request, CancellationToken cancellationToken)
    {        
        var command = new UpdateClientCommand() { Parametr = request.Client };
        
        var client = await commandExecutor.Execute(command, cancellationToken);        

        return new UpdateClientResponse()
        {
            Data = client
        };
    }
}
