namespace CreateInvoiceSystem.Addresses.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Addresses.Application.Commands;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.UpdateAddress;
using MediatR;


public class UpdateAddressHandler(ICommandExecutor commandExecutor) : IRequestHandler<UpdateClientRequest, UpdateClientResponse>
{    
    public async Task<UpdateClientResponse> Handle(UpdateClientRequest request, CancellationToken cancellationToken)
    {        
        var command = new UpdateClientCommand() { Parametr = request.Address };
        
        var address = await commandExecutor.Execute(command, cancellationToken);        

        return new UpdateClientResponse()
        {
            Data = address
        };
    }
}
