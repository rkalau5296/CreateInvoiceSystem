namespace CreateInvoiceSystem.Address.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Address.Application.Commands;
using CreateInvoiceSystem.Address.Application.RequestsResponses.UpdateAddress;
using MediatR;


public class UpdateAddressHandler(ICommandExecutor commandExecutor) : IRequestHandler<UpdateAddressRequest, UpdateAddressResponse>
{    
    public async Task<UpdateAddressResponse> Handle(UpdateAddressRequest request, CancellationToken cancellationToken)
    {        
        var command = new UpdateAddressCommand() { Parametr = request.Address };
        
        var addressFromDb = await commandExecutor.Execute(command, cancellationToken);      
        
        return new UpdateAddressResponse()
        {
            Data = addressFromDb
        };
    }
}
