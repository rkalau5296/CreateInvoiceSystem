namespace CreateInvoiceSystem.Clients.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Clients.Application.Commands;
using CreateInvoiceSystem.Clients.Application.RequestsResponses.UpdateClient;
using CreateInvoiceSystem.Clients.Domain.Entities;
using MediatR;


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
